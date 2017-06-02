﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;

using LifeTimeV3.BL.LifeTimeDiagram;
using LifeTimeV3.LifeTimeDiagram;
using LifeTimeV3.Src;


namespace LifeTimeV3.LifeTimeDiagram.Toolbox.Controls
{
    /// <summary>
    /// Object Browser
    /// </summary>
    public class LifeTimeObjectBrowser : TreeView
    {
        #region properties
        internal LifeTimeObjectTreeNode SelectedObject
        { get; private set; }

        public LifeTimeFindObjectControl FindObjectControl
        { get; private set; }
        #endregion

        #region fields
        private LifeTimeDiagramEditor.LifeTimeGroup _root;
        private Dictionary<int, LifeTimeDiagramEditor.ILifeTimeObject> _objectsByIndex;
        private Dictionary<LifeTimeDiagramEditor.ILifeTimeObject, TreeNode> _treenodesByObject;
        private LifeTimeDiagramEditor.ILifeTimeObject _selectedObject;
        private LifeTimeObjectTreeNode _selectedNode;
        private int _findResultsIndex;
        #endregion

        #region Constructor
        public LifeTimeObjectBrowser()
        {
            _objectsByIndex = new Dictionary<int, LifeTimeDiagramEditor.ILifeTimeObject>();
            _treenodesByObject = new Dictionary<LifeTimeDiagramEditor.ILifeTimeObject, TreeNode>();
            FindObjectControl = new LifeTimeFindObjectControl();
            FindObjectControl.SearchTextChanged += new KeyEventHandler(searchText_Changed);
            FindObjectControl.FindNextOrPrev += new LifeTimeFindObjectControl.FindEventHandler(FindNextOrPrevious_Clicked);

            this.HideSelection = false;
            this.CheckBoxes = true;

            this.AfterSelect += new TreeViewEventHandler(TreeViewObjectSelected);
            this.AfterCheck += new TreeViewEventHandler(CheckedStateChanged);            

            this.ImageList = new ImageList();
            this.ImageList.Images.Add(Properties.Resources._folder); //0
            this.ImageList.Images.Add(Properties.Resources._timespan); //1
            this.ImageList.Images.Add(Properties.Resources._marker); //2
            this.ImageList.Images.Add(Properties.Resources._event); //3
            this.ImageList.Images.Add(Properties.Resources._text); //4

            this.SelectedImageIndex = 4;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Update Object Browser and load new content
        /// </summary>
        /// <param name="group"></param>
        public void UpdateObjectBrowser(LifeTimeDiagramEditor.LifeTimeGroup group)
        {
            this.BeginUpdate();

            _root = group;

            this.Nodes.Clear();
            _objectsByIndex.Clear();
            _treenodesByObject.Clear();

            int index = 0;

            LifeTimeObjectTreeNode root = new LifeTimeObjectTreeNode(_root, true);
            root.Expand();
            root.Text = LifeTimeV3TextList.GetText("[101]");
            root.Name = (-1).ToString();
            root.ImageIndex = 0;
            root.SelectedImageIndex = 0;
            root.Checked = true;

            root.NodeChanged += new LifeTimeObjectTreeNode.NodeChangedEvent(NodeChangedEvent);
            root.CollapsAllExpandAllRequested += new LifeTimeObjectTreeNode.CollapsAllExpandAllRequestEvent(CollapseExpandRequest);

            UpdateTreeViewDeep(root, _root.Groups, ref index);

            this.Nodes.Add(root);

            if (_selectedNode != null) ShowItemInObjectBrowser(_selectedNode);

            this.EndUpdate();
        }

        /// <summary>
        /// Update ObjectBrowser
        /// </summary>
        public void UpdateObjectBrowser()
        {
            this.BeginUpdate();

            foreach (LifeTimeObjectTreeNode n in this.Nodes)
                UpdateTreeViewDeep(n);

            this.EndUpdate();
        }

        /// <summary>
        /// Expand all parent nodes to show the given object
        /// </summary>
        /// <param name="o"></param>
        public LifeTimeObjectTreeNode ShowItemInObjectBrowser(LifeTimeDiagramEditor.ILifeTimeObject o)
        {
            if (o == null) return null;

            TreeNode t = null;

            _treenodesByObject.TryGetValue(o, out t);

            if (t != null) ShowItemInObjectBrowser(t);

            SelectedObject = t as LifeTimeObjectTreeNode;

            ItemSelectedArgs args = new ItemSelectedArgs(o);
            ItemSelected?.Invoke(this, args);
            
            return t as LifeTimeObjectTreeNode;
        }

        /// <summary>
        /// Expand all parent nodes to show the given node
        /// </summary>
        /// <param name="t"></param>
        public void ShowItemInObjectBrowser(TreeNode t)
        {
            this.SelectedNode = t;
        }

        /// <summary>
        /// Return all Nodes matching the search string
        /// </summary>
        /// <param name="findText"></param>
        /// <returns></returns>
        public List<TreeNode> SearchInTreeNodes(string findText)
        {
            return deepSearchInTreeNodes(Nodes, findText);
        }

        private List<TreeNode> deepSearchInTreeNodes(TreeNodeCollection treeNodeCollection, string findText)
        {
            List<TreeNode> coll = new List<TreeNode>();

            foreach (TreeNode t in treeNodeCollection)
            {
                if (t.Text.Contains(findText))
                    coll.Add(t);

                coll.AddRange(deepSearchInTreeNodes(t.Nodes, findText));
            }

            return coll;
        }
        #endregion

        #region private Methods
        private void UpdateTreeViewDeep(LifeTimeObjectTreeNode parent, List<LifeTimeDiagramEditor.LifeTimeGroup> g, ref int i)
        {
            foreach (LifeTimeDiagramEditor.LifeTimeGroup _g in g)
            {
                if (!_g.Deleted)
                {
                    LifeTimeObjectTreeNode grpNode = AddGroupNode(_g, ref i);

                    foreach (LifeTimeDiagramEditor.LifeTimeElement _o in _g.Objects)
                    {
                        if (!_o.Deleted) AddElementNode(_o, grpNode, ref i);
                    }

                    UpdateTreeViewDeep(grpNode, _g.Groups, ref i);

                    parent.Nodes.Add(grpNode);
                }
            }
        }

        private LifeTimeObjectTreeNode AddGroupNode(LifeTimeDiagramEditor.LifeTimeGroup _g, ref int i)
        {
            LifeTimeObjectTreeNode grpNode = new LifeTimeObjectTreeNode(_g, false);
            grpNode.Name = i.ToString();
            grpNode.Checked = _g.Enabled;
            grpNode.Text = string.Format("{0}", _g.Name);
            //grpNode.NodeFont = new Font("Arial", 10.0f, FontStyle.Bold);
            grpNode.ImageIndex = 0;
            grpNode.SelectedImageIndex = 0;
            grpNode.NodeChanged += new LifeTimeObjectTreeNode.NodeChangedEvent(NodeChangedEvent);
            grpNode.CollapsAllExpandAllRequested += new LifeTimeObjectTreeNode.CollapsAllExpandAllRequestEvent(CollapseExpandRequest);

            _objectsByIndex.Add(i, _g);
            _treenodesByObject.Add(_g, grpNode);

            if (_selectedObject as LifeTimeDiagramEditor.LifeTimeGroup == _g) _selectedNode = grpNode;

            i++;

            return grpNode;
        }

        private void AddElementNode(LifeTimeDiagramEditor.LifeTimeElement _o, LifeTimeObjectTreeNode _g, ref int i)
        {
            LifeTimeObjectTreeNode objNode = new LifeTimeObjectTreeNode(_o, false);
            objNode.Name = i.ToString();
            objNode.Checked = _o.Enabled;
            objNode.Text = string.Format("{0}", _o.Name);
            //objNode.NodeFont = new Font("Arial", 10.0f, FontStyle.Regular);
            objNode.NodeChanged += new LifeTimeObjectTreeNode.NodeChangedEvent(NodeChangedEvent);
            objNode.CollapsAllExpandAllRequested += new LifeTimeObjectTreeNode.CollapsAllExpandAllRequestEvent(CollapseExpandRequest);

            if (_o.Type == LifeTimeDiagramEditor.LifeTimeElement.LifeTimeObjectType.TimeSpan) objNode.ImageIndex = 1;
            if (_o.Type == LifeTimeDiagramEditor.LifeTimeElement.LifeTimeObjectType.Marker) objNode.ImageIndex = 2;
            if (_o.Type == LifeTimeDiagramEditor.LifeTimeElement.LifeTimeObjectType.Event) objNode.ImageIndex = 3;
            if (_o.Type == LifeTimeDiagramEditor.LifeTimeElement.LifeTimeObjectType.Text) objNode.ImageIndex = 4;

            objNode.SelectedImageIndex = objNode.ImageIndex;

            _objectsByIndex.Add(i, _o);
            _treenodesByObject.Add(_o, objNode);

            _g.Nodes.Add(objNode);

            if (_selectedObject as LifeTimeDiagramEditor.LifeTimeElement == _o) _selectedNode = objNode;

            i++;
        }

        private void UpdateTreeViewDeep(LifeTimeObjectTreeNode n)
        {
            if (n.Text != n.Object.Name) n.Text = n.Object.Name;

            if (n.Object is LifeTimeDiagramEditor.LifeTimeElement)
            {
                LifeTimeDiagramEditor.LifeTimeElement o = n.Object as LifeTimeDiagramEditor.LifeTimeElement;

                if (o.Type == LifeTimeDiagramEditor.LifeTimeElement.LifeTimeObjectType.TimeSpan && n.ImageIndex != 1)
                    n.ImageIndex = 1;
                if (o.Type == LifeTimeDiagramEditor.LifeTimeElement.LifeTimeObjectType.Marker && n.ImageIndex != 2)
                    n.ImageIndex = 2;
                if (o.Type == LifeTimeDiagramEditor.LifeTimeElement.LifeTimeObjectType.Event && n.ImageIndex != 3)
                    n.ImageIndex = 3;
                if (o.Type == LifeTimeDiagramEditor.LifeTimeElement.LifeTimeObjectType.Text && n.ImageIndex != 4)
                    n.ImageIndex = 4;
            }


            foreach (LifeTimeObjectTreeNode _n in n.Nodes)
                UpdateTreeViewDeep(_n);
        }
        #endregion

        #region events
        public delegate void ItemSelectedHandler(object sender, ItemSelectedArgs e);
        public event ItemSelectedHandler ItemSelected;        
        public event EventHandler ObjectCollectionChanged;

        public class ItemSelectedArgs
        {
            public LifeTimeDiagramEditor.ILifeTimeObject Object { get; private set; }
            public ItemSelectedArgs(LifeTimeDiagramEditor.ILifeTimeObject o)
            {
                Object = o;                
            }
        }
        #endregion

        #region event handler
        private void TreeViewObjectSelected(object sender, TreeViewEventArgs e)
        {
            TreeNode n = this.SelectedNode;
            SelectedObject = n as LifeTimeObjectTreeNode;

            this.ContextMenuStrip = n.ContextMenuStrip;

            LifeTimeDiagramEditor.ILifeTimeObject o;

            _objectsByIndex.TryGetValue(Convert.ToInt16(n.Name), out o);

            ItemSelectedArgs args = new ItemSelectedArgs(o);
            if (ItemSelected != null && o != null) this.ItemSelected(this, args);
        }

        private void NodeChangedEvent(object sender, LifeTimeObjectTreeNode.NodeChangedEventArgs e)
        {
            LifeTimeObjectTreeNode t = sender as LifeTimeObjectTreeNode;
            SelectedObject = t;
            _selectedObject = e.NewObject;

            UpdateObjectBrowser(_root);

            ItemSelectedArgs args = new ItemSelectedArgs(e.NewObject);
            if (ItemSelected != null && e.NewObject != null) this.ItemSelected(this, args);
            if (ObjectCollectionChanged != null) this.ObjectCollectionChanged(this, null);
        }

        private void CollapseExpandRequest(object sender, LifeTimeObjectTreeNode.CollapseAllExpandAllEventArgs e)
        {
            if (e.Request == LifeTimeObjectTreeNode.CollapseAllExpandAllEventArgs.RequestType.CollapseAll) this.CollapseAll();
            if (e.Request == LifeTimeObjectTreeNode.CollapseAllExpandAllEventArgs.RequestType.ExpandAll) this.ExpandAll();
        }

        private void CheckedStateChanged(object sender, TreeViewEventArgs e)
        {
            if (e.Action == TreeViewAction.ByMouse || e.Action == TreeViewAction.ByKeyboard)
            {
                LifeTimeObjectTreeNode n = e.Node as LifeTimeObjectTreeNode;

                n.Object.Enabled = n.Checked;

                if (ObjectCollectionChanged != null) ObjectCollectionChanged(n.Object, e);
            }
        }

        private void searchText_Changed(object sender, KeyEventArgs e)
        {
            _findResultsIndex = 0;
            List<TreeNode> r = SearchInTreeNodes(FindObjectControl.SearchText);
            if (r.Count != 0)
            {
                ShowItemInObjectBrowser(r[_findResultsIndex]);
                FindObjectControl.UpdateStatusLabel(r.Count, _findResultsIndex + 1);
            }
            else
            {
                FindObjectControl.UpdateStatusLabel(0,0);
            }
            
        }

        private void FindNextOrPrevious_Clicked(object sender, LifeTimeFindObjectControl.FindEventArgs n)
        {
            List<TreeNode> r = SearchInTreeNodes(FindObjectControl.SearchText);

            _findResultsIndex += n.NextItem;

            if (_findResultsIndex > r.Count - 1)
                _findResultsIndex = 0;
            if (_findResultsIndex < 0)
                _findResultsIndex = r.Count - 1;

            ShowItemInObjectBrowser(r[_findResultsIndex]);
            FindObjectControl.UpdateStatusLabel(r.Count, _findResultsIndex + 1);
        }

        #endregion

        #region LifeTimeObjectTreeNode Class
        public class LifeTimeObjectTreeNode : TreeNode
        {
            #region Properties
            public LifeTimeDiagramEditor.ILifeTimeObject Object
            {
                get { return _object; }
            }
            #endregion

            #region fields
            private LifeTimeDiagramEditor.ILifeTimeObject _object;
            #endregion

            #region constructor
            public LifeTimeObjectTreeNode(LifeTimeDiagramEditor.ILifeTimeObject o, Boolean IsRoot)
            {
                _object = o;                

                this.ContextMenuStrip = new ContextMenuStrip();

                foreach (ToolStripItem i in BuildContextMenu(IsRoot, false))
                    this.ContextMenuStrip.Items.Add(i);                
            }
            #endregion

            public List<ToolStripItem> BuildContextMenu(bool IsRoot, bool IsMainMenu)
            {
                List<ToolStripItem> contextMenuItemCollection = new List<ToolStripItem>();

                if (_object is LifeTimeDiagramEditor.LifeTimeGroup)
                {
                    if (!IsRoot) AddContextMenuItem(contextMenuItemCollection, ContextMenuItems.AddElement, true);
                    AddContextMenuItem(contextMenuItemCollection, ContextMenuItems.AddGroup, false);
                    AddContextMenuItem(contextMenuItemCollection, ContextMenuItems.Separator, false);
                    if (!IsRoot) AddContextMenuItem(contextMenuItemCollection, ContextMenuItems.Delete, false);
                    if (!IsRoot) AddContextMenuItem(contextMenuItemCollection, ContextMenuItems.Separator, false);
                    if (!IsRoot) AddContextMenuItem(contextMenuItemCollection, ContextMenuItems.Paste, false);
                    if (!IsRoot) AddContextMenuItem(contextMenuItemCollection, ContextMenuItems.Separator, false);
                    if (!IsRoot) AddContextMenuItem(contextMenuItemCollection, ContextMenuItems.MoveUp, false);
                    if (!IsRoot) AddContextMenuItem(contextMenuItemCollection, ContextMenuItems.MoveDown, false);
                    if (!IsRoot) AddContextMenuItem(contextMenuItemCollection, ContextMenuItems.Separator, false);
                    AddContextMenuItem(contextMenuItemCollection, ContextMenuItems.ExpandAll, false);
                    AddContextMenuItem(contextMenuItemCollection, ContextMenuItems.CollapseAll, false);
                }

                if (_object is LifeTimeDiagramEditor.LifeTimeElement)
                {
                    AddContextMenuItem(contextMenuItemCollection, ContextMenuItems.Copy, true);
                    AddContextMenuItem(contextMenuItemCollection, ContextMenuItems.CopyPeriodic, true);
                    AddContextMenuItem(contextMenuItemCollection, ContextMenuItems.Cut, true);
                    AddContextMenuItem(contextMenuItemCollection, ContextMenuItems.Paste, true);
                    AddContextMenuItem(contextMenuItemCollection, ContextMenuItems.Separator, true);
                    AddContextMenuItem(contextMenuItemCollection, ContextMenuItems.Delete, true);
                    AddContextMenuItem(contextMenuItemCollection, ContextMenuItems.Separator, true);
                    AddContextMenuItem(contextMenuItemCollection, ContextMenuItems.BringToFront, true);
                    AddContextMenuItem(contextMenuItemCollection, ContextMenuItems.MoveUp, true);
                    AddContextMenuItem(contextMenuItemCollection, ContextMenuItems.MoveDown, true);
                    AddContextMenuItem(contextMenuItemCollection, ContextMenuItems.BringToBack, true);
                    if (!IsMainMenu) AddContextMenuItem(contextMenuItemCollection, ContextMenuItems.Separator, true);
                    if (!IsMainMenu) AddContextMenuItem(contextMenuItemCollection, ContextMenuItems.ExpandAll, true);
                    if (!IsMainMenu) AddContextMenuItem(contextMenuItemCollection, ContextMenuItems.CollapseAll, true);
                }

                return contextMenuItemCollection;
            }

            public enum ContextMenuItems { Separator, CopyPeriodic, AddElement, AddGroup, Delete, Cut, Copy, Paste, MoveUp, MoveDown, BringToFront, BringToBack, CollapseAll, ExpandAll }
            public void AddContextMenuItem(List<ToolStripItem> toolStripItemCollection, ContextMenuItems item, bool toMainMenu)
            {
                //SEPARATOR
                if (item == ContextMenuItems.Separator)
                {
                    toolStripItemCollection.Add(new ToolStripSeparator());                 
                }

                //COPY PERIODIC
                if (item == ContextMenuItems.CopyPeriodic)
                {
                    ToolStripItem i = new ToolStripMenuItem(LifeTimeV3TextList.GetText("[218]"));
                    i.Click += new EventHandler(MenuItemCopyPeriodicClicked);
                    toolStripItemCollection.Add(i);
                }

                //ADD ELEMENT
                if (item == ContextMenuItems.AddElement)
                {
                    ToolStripItem i = new ToolStripMenuItem(LifeTimeV3TextList.GetText("[200]"));
                    i.Click += new EventHandler(MenuItemAddElementClicked);
                    toolStripItemCollection.Add(i);                    
                    (toolStripItemCollection[toolStripItemCollection.Count - 1] as ToolStripMenuItem).ShortcutKeys = Keys.Control | Keys.N;
                }

                //ADD GROUP
                if (item == ContextMenuItems.AddGroup)
                {
                    ToolStripItem i = new ToolStripMenuItem(LifeTimeV3TextList.GetText("[201]"));
                    i.Click += new EventHandler(MenuItemAddGroupClicked);
                    toolStripItemCollection.Add(i);
                }

                //DELETE
                if (item == ContextMenuItems.Delete)
                {
                    ToolStripItem i = new ToolStripMenuItem(LifeTimeV3TextList.GetText("[203]"));
                    i.Click += new EventHandler(MenuItemDeleteItemClicked);
                    toolStripItemCollection.Add(i);
                    (toolStripItemCollection[toolStripItemCollection.Count - 1] as ToolStripMenuItem).ShortcutKeys = Keys.Delete;
                }

                //COPY
                if (item == ContextMenuItems.Copy)
                {
                    ToolStripItem i = new ToolStripMenuItem(LifeTimeV3TextList.GetText("[204]"));
                    i.Click += new EventHandler(MenuItemCopyClicked);
                    toolStripItemCollection.Add(i);                    
                    (toolStripItemCollection[toolStripItemCollection.Count - 1] as ToolStripMenuItem).ShortcutKeys = Keys.Control | Keys.C;
                }

                //CUT
                if (item == ContextMenuItems.Cut)
                {
                    ToolStripItem i = new ToolStripMenuItem(LifeTimeV3TextList.GetText("[205]"));
                    i.Click += new EventHandler(MenuItemCutClicked);
                    toolStripItemCollection.Add(i);                    
                    (toolStripItemCollection[toolStripItemCollection.Count - 1] as ToolStripMenuItem).ShortcutKeys = Keys.Control | Keys.X;
                }

                //PASTE
                if (item == ContextMenuItems.Paste)
                {
                    ToolStripItem i = new ToolStripMenuItem(LifeTimeV3TextList.GetText("[206]"));
                    i.Click += new EventHandler(MenuItemPasteClicked);
                    toolStripItemCollection.Add(i);                    
                    (toolStripItemCollection[toolStripItemCollection.Count - 1] as ToolStripMenuItem).ShortcutKeys = Keys.Control | Keys.V;
                }

                //BRING TO FRONT
                if (item == ContextMenuItems.BringToFront)
                {
                    ToolStripItem i = new ToolStripMenuItem(LifeTimeV3TextList.GetText("[207]"));
                    i.Click += new EventHandler(MenuItemBringToFront);
                    toolStripItemCollection.Add(i);
                    (toolStripItemCollection[toolStripItemCollection.Count - 1] as ToolStripMenuItem).ShortcutKeys = Keys.Control | Keys.Shift | Keys.Up;
                }

                //MOVE UP
                if (item == ContextMenuItems.MoveUp)
                {
                    ToolStripItem i = new ToolStripMenuItem(LifeTimeV3TextList.GetText("[208]"));
                    i.Click += new EventHandler(MenuItemMovetoFront);
                    toolStripItemCollection.Add(i);                    
                    (toolStripItemCollection[toolStripItemCollection.Count - 1] as ToolStripMenuItem).ShortcutKeys = Keys.Control | Keys.Up;
                }

                //MOVE DOWN
                if (item == ContextMenuItems.MoveDown)
                {
                    ToolStripItem i = new ToolStripMenuItem(LifeTimeV3TextList.GetText("[209]"));
                    i.Click += new EventHandler(MenuItemMovetoBack);
                    toolStripItemCollection.Add(i);                    
                    (toolStripItemCollection[toolStripItemCollection.Count - 1] as ToolStripMenuItem).ShortcutKeys = Keys.Control | Keys.Down;                    
                }

                //BRING TO BACK
                if (item == ContextMenuItems.BringToBack)
                {
                    ToolStripItem i = new ToolStripMenuItem(LifeTimeV3TextList.GetText("[210]"));
                    i.Click += new EventHandler(MenuItemBringToBack);
                    toolStripItemCollection.Add(i);                    
                    (toolStripItemCollection[toolStripItemCollection.Count - 1] as ToolStripMenuItem).ShortcutKeys = Keys.Control | Keys.Shift | Keys.Down;                    
                }

                //EXPAND ALL
                if (item == ContextMenuItems.ExpandAll)
                {
                    ToolStripItem i = new ToolStripMenuItem(LifeTimeV3TextList.GetText("[211]"));
                    i.Click += new EventHandler(MenuItemExpandAll);
                    toolStripItemCollection.Add(i);                    
                }

                //COLLAPSE ALL
                if (item == ContextMenuItems.CollapseAll)
                {
                    ToolStripItem i = new ToolStripMenuItem(LifeTimeV3TextList.GetText("[212]"));
                    i.Click += new EventHandler(MenuItemCollapsedAll);
                    toolStripItemCollection.Add(i);                    
                }
            }

            #region private methods
            private void CopyObjectToClipboard(LifeTimeDiagramEditor.ILifeTimeObject o)
            {
                Clipboard.Clear();
                LifeTimeDiagramEditor.LifeTimeXmlObject xml = new LifeTimeDiagramEditor.LifeTimeXmlObject(new XmlDocument());
                XmlNode n = null;
                if (_object is LifeTimeDiagramEditor.LifeTimeElement)
                {
                    n = xml.GetXmlFromObject(_object as LifeTimeDiagramEditor.LifeTimeElement);
                    Clipboard.SetData("LifeTimeElement", n.OuterXml);
                }
                else if (_object is LifeTimeDiagramEditor.LifeTimeGroup)
                {
                    n = xml.GetXmlFromObject(_object as LifeTimeDiagramEditor.LifeTimeGroup);
                    Clipboard.SetData("LifeTimeGroup", n.OuterXml);
                }
            }
            #endregion

            #region events
            private void MenuItemAddElementClicked(object sender, EventArgs e)
            {
                LifeTimeDiagramEditor.LifeTimeGroup o = Object as LifeTimeDiagramEditor.LifeTimeGroup;

                LifeTimeDiagramEditor.LifeTimeElement newObj = new LifeTimeDiagramEditor.LifeTimeElement(LifeTimeV3TextList.GetText("[102]"), LifeTimeDiagramEditor.LifeTimeElement.LifeTimeObjectType.Event); //new element

                o.Objects.Add(newObj);

                NodeChangedEventArgs eventArgs = new NodeChangedEventArgs();
                eventArgs.NewObject = newObj;

                NodeChanged?.Invoke(this, eventArgs);
            }

            private void MenuItemAddGroupClicked(object sender, EventArgs e)
            {
                LifeTimeDiagramEditor.LifeTimeGroup o = Object as LifeTimeDiagramEditor.LifeTimeGroup;

                LifeTimeDiagramEditor.LifeTimeGroup newObj = new LifeTimeDiagramEditor.LifeTimeGroup(LifeTimeV3TextList.GetText("[103]"), Color.White); //new group

                o.Groups.Add(newObj);

                NodeChangedEventArgs eventArgs = new NodeChangedEventArgs();
                eventArgs.NewObject = newObj;

                NodeChanged?.Invoke(this, eventArgs);
            }

            private void MenuItemDeleteItemClicked(object sender, EventArgs e)
            {
                DialogResult r = DialogResult.No;
                if (_object is LifeTimeDiagramEditor.LifeTimeElement) r = MessageBox.Show(string.Format(LifeTimeV3TextList.GetText("[300]"), _object.Name), "", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (_object is LifeTimeDiagramEditor.LifeTimeGroup) r = MessageBox.Show(string.Format(LifeTimeV3TextList.GetText("[301]"), _object.Name), "", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (r == DialogResult.No) return;

                DeleteObject(_object);

                NodeChangedEventArgs eventArgs = new NodeChangedEventArgs();
                eventArgs.NewObject = null;

                NodeChanged?.Invoke(this, eventArgs);
            }

            private void DeleteObject(LifeTimeDiagramEditor.ILifeTimeObject g)
            {
                g.Deleted = true;
                foreach (LifeTimeDiagramEditor.ILifeTimeObject o in g.Objects)
                {
                    o.Deleted = true;
                }
                foreach (LifeTimeDiagramEditor.ILifeTimeObject o in g.Groups)
                {
                    o.Deleted = true;
                    DeleteObject(o);
                }
            }

            private void MenuItemPasteClicked(object sender, EventArgs e)
            {
                LifeTimeDiagramEditor.ILifeTimeObject o = null;
                XmlDocument xmldoc = new XmlDocument();
                LifeTimeDiagramEditor.LifeTimeXmlObject xml = new LifeTimeDiagramEditor.LifeTimeXmlObject(xmldoc);
                XmlNode n = xmldoc.CreateElement("object");

                if (Clipboard.ContainsData("LifeTimeElement"))
                {
                    n.InnerXml = (string)Clipboard.GetData("LifeTimeElement");
                    o = xml.GetObjectFromXml(n.FirstChild) as LifeTimeDiagramEditor.LifeTimeElement;

                    LifeTimeDiagramEditor.LifeTimeGroup g = null;
                    if (Object is LifeTimeDiagramEditor.LifeTimeGroup) g = Object as LifeTimeDiagramEditor.LifeTimeGroup;
                    if (Object is LifeTimeDiagramEditor.LifeTimeElement) g = (this.Parent as LifeTimeObjectTreeNode).Object as LifeTimeDiagramEditor.LifeTimeGroup;
                    g.Objects.Add(o as LifeTimeDiagramEditor.LifeTimeElement);
                }
                else if (Clipboard.ContainsData("LifeTimeGroup"))
                {
                    n.InnerXml = (string)Clipboard.GetData("LifeTimeGroup");
                    o = xml.GetObjectFromXml(n.FirstChild) as LifeTimeDiagramEditor.LifeTimeGroup;

                    LifeTimeDiagramEditor.LifeTimeGroup g = null;
                    if (Object is LifeTimeDiagramEditor.LifeTimeGroup) g = Object as LifeTimeDiagramEditor.LifeTimeGroup;
                    if (Object is LifeTimeDiagramEditor.LifeTimeElement) g = (this.Parent as LifeTimeObjectTreeNode).Object as LifeTimeDiagramEditor.LifeTimeGroup;
                    g.Groups.Add(o as LifeTimeDiagramEditor.LifeTimeGroup);
                }

                NodeChangedEventArgs eventArgs = new NodeChangedEventArgs();
                eventArgs.NewObject = o;

                NodeChanged?.Invoke(this, eventArgs);
            }

            private void MenuItemCutClicked(object sender, EventArgs e)
            {
                CopyObjectToClipboard(_object);

                DeleteObject(_object);

                NodeChangedEventArgs eventArgs = new NodeChangedEventArgs();
                eventArgs.NewObject = null;

                NodeChanged?.Invoke(this, eventArgs);
            }

            private void MenuItemCopyClicked(object sender, EventArgs e)
            {
                CopyObjectToClipboard(_object);
            }

            private void MenuItemCopyPeriodicClicked(object sender, EventArgs e)
            {                
                DiagramBox.CopyPeriodicDialog.FormCopyPeriodicDialog CopyPeriodicDialog = new DiagramBox.CopyPeriodicDialog.FormCopyPeriodicDialog(_object);
                DialogResult d = CopyPeriodicDialog.ShowDialog();
                
                if (d == DialogResult.Cancel)
                    return;
                
                LifeTimeDiagramEditor.LifeTimeGroup g = null;
                if (Object is LifeTimeDiagramEditor.LifeTimeGroup) g = Object as LifeTimeDiagramEditor.LifeTimeGroup;
                if (Object is LifeTimeDiagramEditor.LifeTimeElement) g = (this.Parent as LifeTimeObjectTreeNode).Object as LifeTimeDiagramEditor.LifeTimeGroup;

                if(CopyPeriodicDialog.UseLimitForAddingCopies)
                    foreach (LifeTimeDiagramEditor.LifeTimeElement o in LifeTimeDiagramEditor.MultiplyElements(_object, CopyPeriodicDialog.PeriodBase, CopyPeriodicDialog.Period, CopyPeriodicDialog.LimitForAddingCopies))
                        g.Objects.Add(o);
                else
                    foreach (LifeTimeDiagramEditor.LifeTimeElement o in LifeTimeDiagramEditor.MultiplyElements(_object, CopyPeriodicDialog.PeriodBase, CopyPeriodicDialog.Period, CopyPeriodicDialog.AmmountOfCopies))
                        g.Objects.Add(o);

                NodeChangedEventArgs eventArgs = new NodeChangedEventArgs();
                eventArgs.NewObject = null;                

                NodeChanged?.Invoke(this, eventArgs);
            }

            private void MenuItemBringToFront(object sender, EventArgs e)
            {
                LifeTimeObjectTreeNode parent = (this.Parent as LifeTimeObjectTreeNode);

                for (int i = 0; i < parent.Object.Objects.Count - 1; i++)
                {
                    if (parent.Object.Objects[i] == Object)
                    {
                        parent.Object.Objects.RemoveAt(i);
                        parent.Object.Objects.Add(Object as LifeTimeDiagramEditor.LifeTimeElement);
                        break;
                    }
                }

                NodeChangedEventArgs eventArgs = new NodeChangedEventArgs();
                eventArgs.NewObject = Object;

                if (NodeChanged != null) NodeChanged(this, eventArgs);
            }

            private void MenuItemMovetoFront(object sender, EventArgs e)
            {
                LifeTimeObjectTreeNode parent = (this.Parent as LifeTimeObjectTreeNode);

                if (Object is LifeTimeDiagramEditor.LifeTimeElement)
                {
                    for (int i = 0; i < parent.Object.Objects.Count - 1; i++)
                    {
                        if (parent.Object.Objects[i] == Object)
                        {
                            parent.Object.Objects.RemoveAt(i);
                            parent.Object.Objects.Insert(i + 1, Object as LifeTimeDiagramEditor.LifeTimeElement);
                            break;
                        }
                    }
                }
                else if (Object is LifeTimeDiagramEditor.LifeTimeGroup)
                {
                    for (int i = 0; i < parent.Object.Groups.Count - 1; i++)
                    {
                        if (parent.Object.Groups[i] == Object)
                        {
                            parent.Object.Groups.RemoveAt(i);
                            parent.Object.Groups.Insert(i + 1, Object as LifeTimeDiagramEditor.LifeTimeGroup);
                            break;
                        }
                    }
                }

                NodeChangedEventArgs eventArgs = new NodeChangedEventArgs();
                eventArgs.NewObject = Object;

                if (NodeChanged != null) NodeChanged(this, eventArgs);
            }

            private void MenuItemBringToBack(object sender, EventArgs e)
            {
                LifeTimeObjectTreeNode parent = (this.Parent as LifeTimeObjectTreeNode);

                for (int i = 0; i <= parent.Object.Objects.Count - 1; i++)
                {
                    if (parent.Object.Objects[i] == Object)
                    {
                        parent.Object.Objects.RemoveAt(i);
                        parent.Object.Objects.Insert(0, Object as LifeTimeDiagramEditor.LifeTimeElement);
                        break;
                    }
                }

                NodeChangedEventArgs eventArgs = new NodeChangedEventArgs();
                eventArgs.NewObject = Object;

                if (NodeChanged != null) NodeChanged(this, eventArgs);
            }

            private void MenuItemMovetoBack(object sender, EventArgs e)
            {
                LifeTimeObjectTreeNode parent = (this.Parent as LifeTimeObjectTreeNode);

                if (Object is LifeTimeDiagramEditor.LifeTimeElement)
                {
                    for (int i = 1; i <= parent.Object.Objects.Count - 1; i++)
                    {
                        if (parent.Object.Objects[i] == Object)
                        {
                            parent.Object.Objects.RemoveAt(i);
                            parent.Object.Objects.Insert(i - 1, Object as LifeTimeDiagramEditor.LifeTimeElement);
                            break;
                        }
                    }
                }
                else if (Object is LifeTimeDiagramEditor.LifeTimeGroup)
                {
                    for (int i = 1; i <= parent.Object.Groups.Count - 1; i++)
                    {
                        if (parent.Object.Groups[i] == Object)
                        {
                            parent.Object.Groups.RemoveAt(i);
                            parent.Object.Groups.Insert(i - 1, Object as LifeTimeDiagramEditor.LifeTimeGroup);
                            break;
                        }
                    }
                }

                NodeChangedEventArgs eventArgs = new NodeChangedEventArgs();
                eventArgs.NewObject = Object;

                if (NodeChanged != null) NodeChanged(this, eventArgs);
            }

            private void MenuItemCollapsedAll(object sender, EventArgs e)
            {
                CollapseAllExpandAllEventArgs args = new CollapseAllExpandAllEventArgs();
                args.Request = CollapseAllExpandAllEventArgs.RequestType.CollapseAll;
                if (CollapsAllExpandAllRequested != null) CollapsAllExpandAllRequested(this, args);
            }

            private void MenuItemExpandAll(object sender, EventArgs e)
            {
                CollapseAllExpandAllEventArgs args = new CollapseAllExpandAllEventArgs();
                args.Request = CollapseAllExpandAllEventArgs.RequestType.ExpandAll;
                if (CollapsAllExpandAllRequested != null) CollapsAllExpandAllRequested(this, args);
            }
            #endregion

            #region NodeChangedEvent
            public delegate void CollapsAllExpandAllRequestEvent(object sender, CollapseAllExpandAllEventArgs e);
            public event CollapsAllExpandAllRequestEvent CollapsAllExpandAllRequested;

            public class CollapseAllExpandAllEventArgs : EventArgs
            {
                public enum RequestType { CollapseAll, ExpandAll }
                public RequestType Request { get; set; }

                public CollapseAllExpandAllEventArgs()
                { }
            }

            public delegate void NodeChangedEvent(object sender, NodeChangedEventArgs e);
            public event NodeChangedEvent NodeChanged;

            public class NodeChangedEventArgs : EventArgs
            {
                public LifeTimeDiagramEditor.ILifeTimeObject NewObject { get; set; }

                public NodeChangedEventArgs()
                { }
            }

            #endregion
        }
        #endregion

        #region Find Objects Class
        public class LifeTimeFindObjectControl: TableLayoutPanel
        {
            #region Properties    
            public string SearchText
            { get; private set; } 
            #endregion

            #region constructor
            public LifeTimeFindObjectControl() : base()
            {
                RowCount = 2;
                ColumnCount = 3;

                TextBox searchText = new TextBox();                
                searchText.Name = "searchText";
                searchText.Dock = DockStyle.Fill;
                searchText.KeyUp += new KeyEventHandler(searchTextBox_KeyPressed);
                Controls.Add(searchText);
                SetColumn(searchText, 1);
                SetRow(searchText, 1);
                SetColumnSpan(searchText, 3);

                Button fndPrv = new Button();
                fndPrv.Name = "fndPrv";
                fndPrv.Text = LifeTimeV3TextList.GetText("[219]");
                fndPrv.Click += new EventHandler(fndPrv_Clicked);
                Controls.Add(fndPrv);
                SetColumn(fndPrv, 1);
                SetRow(fndPrv, 2);

                Button fndNxt = new Button();
                fndNxt.Name = "fndNxt";
                fndNxt.Text = LifeTimeV3TextList.GetText("[220]");
                fndNxt.Click += new EventHandler(fndNxt_Clicked);
                Controls.Add(fndNxt);
                SetColumn(fndNxt, 2);
                SetRow(fndNxt, 2);

                Label results = new Label();
                results.Name = "results";
                results.Dock = DockStyle.Fill;
                results.TextAlign = ContentAlignment.MiddleLeft;
                Controls.Add(results);
                SetColumn(results, 3);
                SetRow(results, 2);

                Height = searchText.Height + fndPrv.Height + 18;
            }
            #endregion

            #region publics
            public void UpdateStatusLabel(int resultsCount, int selectedResult)
            {
                Label l = (Controls["results"] as Label);

                if (resultsCount == 0)
                {
                    l.Text = LifeTimeV3TextList.GetText("[221]");
                    l.ForeColor = Color.Orange;
                }
                else
                {
                    l.Text = string.Format(LifeTimeV3TextList.GetText("[222]"), selectedResult, resultsCount);
                    l.ForeColor = Color.DarkGreen;
                }
            }
            #endregion

            #region privates
            #endregion
            
            #region eventhandler
            private void fndNxt_Clicked(object sender, EventArgs e)
            {
                FindNextOrPrev?.Invoke(this, new FindEventArgs(+1));
            }

            private void fndPrv_Clicked(object sender, EventArgs e)
            {
                FindNextOrPrev?.Invoke(this, new FindEventArgs(-1));
            }

            private void searchTextBox_KeyPressed(object sender, KeyEventArgs e)
            {
                SearchText = (sender as TextBox).Text;
                SearchTextChanged?.Invoke(this, e);
            }
            #endregion

            #region events
            public event KeyEventHandler SearchTextChanged;

            public delegate void FindEventHandler(object sender, FindEventArgs e);

            public class FindEventArgs : EventArgs
            {
                public int NextItem { get; set; }

                public FindEventArgs(int nextItem)
                {
                    NextItem = nextItem;
                }
            }

            public event FindEventHandler FindNextOrPrev;            
            #endregion
        }
        #endregion
    }

    /// <summary>
    /// PropertyGrid
    /// </summary>
    public class LifeTimeObjectPropertyGrid : TableLayoutPanel
    {
        #region properties
        #endregion

        #region Fields
        private LifeTimeDiagramEditor.ILifeTimeObject _lifeTimeObject;
        private Color ErrorBackColor = Color.Orange;
        private bool _allowDiagramChanging = true;
        #endregion

        #region Constructor
        public LifeTimeObjectPropertyGrid()
        {
            this.AutoScroll = true;
            NoObjectSelected();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Set a LifeTimeObjet
        /// </summary>
        /// <param name="o"></param>
        public void SetObject(LifeTimeDiagramEditor.ILifeTimeObject o)
        {
            _allowDiagramChanging = false;
            this.Controls.Clear();

            if (_lifeTimeObject != null && _lifeTimeObject is LifeTimeDiagramEditor.LifeTimeElement)
            {
                LifeTimeDiagramEditor.LifeTimeElement currObj = _lifeTimeObject as LifeTimeDiagramEditor.LifeTimeElement;
                currObj.Highlight = false;
            }

            if (o == null)
            {
                NoObjectSelected();
                return;
            }
            else if (o is LifeTimeDiagramEditor.LifeTimeElement)
            {
                _lifeTimeObject = o;

                LifeTimeDiagramEditor.LifeTimeElement currObj = _lifeTimeObject as LifeTimeDiagramEditor.LifeTimeElement;

                LoadObjectProperties(currObj);
                currObj.Highlight = true;

                ObjectChangedArgs objChangedArgs = new ObjectChangedArgs();
                objChangedArgs.DiagramChanged = false;
                ObjectChanged?.Invoke(currObj, objChangedArgs);
            }
            else if (o is LifeTimeDiagramEditor.LifeTimeGroup)
            {
                _lifeTimeObject = o;

                LifeTimeDiagramEditor.LifeTimeGroup currObj = _lifeTimeObject as LifeTimeDiagramEditor.LifeTimeGroup;

                LoadObjectProperties(currObj);

                ObjectChangedArgs objChangedArgs = new ObjectChangedArgs();
                objChangedArgs.DiagramChanged = false;
                ObjectChanged?.Invoke(currObj, objChangedArgs);
            }
            else if (o is LifeTimeDiagramEditor.LifeTimeDiagramSettings)
            {
                _lifeTimeObject = o;

                LifeTimeDiagramEditor.LifeTimeDiagramSettings currObj = _lifeTimeObject as LifeTimeDiagramEditor.LifeTimeDiagramSettings;

                LoadObjectProperties(currObj);
            }
            _allowDiagramChanging = true;
        }

        /// <summary>
        /// Get the current LifeTimeObject
        /// </summary>
        /// <returns></returns>
        public LifeTimeDiagramEditor.ILifeTimeObject GetObject()
        {
            return _lifeTimeObject;
        }
        #endregion

        #region private Methods
        private void NoObjectSelected()
        {
            int r = 0;

            Label placeHolder = new Label();
            placeHolder.Text = LifeTimeV3TextList.GetText("[100]"); //No object selected
            this.Controls.Add(placeHolder, 0, r);
        }

        private void LoadObjectProperties<T>(T o)
        {
            Type t = typeof(T);
            int r = 0;

            _allowDiagramChanging = false;
            foreach (string name in (o as LifeTimeDiagramEditor.ILifeTimeObject).Properties(false))
            {
                object value = t.GetProperty(name).GetValue(o);

                AddPropertyLabelToGgrid(name, 0, r);
                AddPropertyControlToGrid(name, value, ref r, t.GetProperty(name).CanWrite);
            }

            AddPropertyLabelToGgrid("", 0, r); //an empty label to finalize the grid
            _allowDiagramChanging = true;
        }

        private void AddPropertyLabelToGgrid(string name, int c, int r)
        {
            Label l = new Label();
            l.Text = LifeTimeV3TextList.GetText(name);
            l.TextAlign = ContentAlignment.MiddleRight;
            this.Controls.Add(l, c, r);
        }

        private void AddPropertyControlToGrid(string name, object value, ref int r, Boolean ro)
        {
            Control c = new Control();
            #region Checkbox
            if (value is bool)
            {
                c = new CheckBox();
                CheckBox d = c as CheckBox;
                d.Name = name;
                d.Text = LifeTimeV3TextList.GetText("[1]"); //Enabled
                d.Checked = (bool)value;

                d.CheckedChanged += new EventHandler(CheckBoxChanged);
            }
            #endregion
            #region Type
            else if (value is LifeTimeDiagramEditor.LifeTimeElement.LifeTimeObjectType)
            {
                c = new TypeSelectorBox();
                TypeSelectorBox d = c as TypeSelectorBox;
                d.Name = name;

                if ((LifeTimeDiagramEditor.LifeTimeElement.LifeTimeObjectType)value == LifeTimeDiagramEditor.LifeTimeElement.LifeTimeObjectType.TimeSpan) d.Text = LifeTimeV3TextList.GetText("[10]");
                if ((LifeTimeDiagramEditor.LifeTimeElement.LifeTimeObjectType)value == LifeTimeDiagramEditor.LifeTimeElement.LifeTimeObjectType.Event) d.Text = LifeTimeV3TextList.GetText("[11]");
                if ((LifeTimeDiagramEditor.LifeTimeElement.LifeTimeObjectType)value == LifeTimeDiagramEditor.LifeTimeElement.LifeTimeObjectType.Marker) d.Text = LifeTimeV3TextList.GetText("[12]");
                if ((LifeTimeDiagramEditor.LifeTimeElement.LifeTimeObjectType)value == LifeTimeDiagramEditor.LifeTimeElement.LifeTimeObjectType.Text) d.Text = LifeTimeV3TextList.GetText("[13]");

                d.TextChanged += new EventHandler(TypeSelectorChanged);
            }
            #endregion
            #region TextBox
            else if (value is string)
            {
                c = new AdvancedTextBox();
                AdvancedTextBox d = c as AdvancedTextBox;
                if (name == "Text")
                {
                    d.EnableMultiline();
                }
                d.Name = name;
                d.Text = (string)value;

                d.ValueChanged += new AdvancedTextBox.ValueChangedHandler(TextBoxChanged);
            }
            #endregion
            #region NumericInt
            else if (value is int)
            {
                c = new AdvancedTextBox();
                AdvancedTextBox d = c as AdvancedTextBox;
                d.Name = name;
                d.Text = ((int)value).ToString();

                d.ValueChanged += new AdvancedTextBox.ValueChangedHandler(TextBoxIntChanged);
            }
            #endregion
            #region NumericDouble
            else if (value is double)
            {
                c = new AdvancedTextBox();
                AdvancedTextBox d = c as AdvancedTextBox;
                d.Name = name;
                d.Text = ((double)value).ToString();

                d.ValueChanged += new AdvancedTextBox.ValueChangedHandler(TextBoxDoubleChanged);
            }
            #endregion
            #region DateTime
            else if (value is DateTime)
            {
                c = new DateTimePicker();
                DateTimePicker d = c as DateTimePicker;
                d.Name = name;
                d.Value = (DateTime)value;

                d.ValueChanged += new EventHandler(DateTimeChanged);
            }
            #endregion
            #region Color
            else if (value is Color)
            {
                c = new ColorSelectorButton();
                ColorSelectorButton d = c as ColorSelectorButton;
                d.Name = name;
                d.Value = (Color)value;

                d.BackColorChanged += new EventHandler(ColorChanged);
            }
            #endregion
            #region What else?
            else
            {
                c = new Label();
                c.Name = name;
                c.Text = LifeTimeV3TextList.GetText("[101]"); //Unknown property
            }
            #endregion

            c.Dock = DockStyle.Fill;
            c.Enabled = ro;
            this.Controls.Add(c, 1, r);
            r++;
        }
        #endregion

        #region Object Changed Event
        public delegate void ObjectChangedEvent(object sender, ObjectChangedArgs e);
        public event ObjectChangedEvent ObjectChanged;
        #endregion

        #region PropertyControls Changed EventHandler
        private void TypeSelectorChanged(object sender, EventArgs e)
        {
            TypeSelectorBox c = sender as TypeSelectorBox;
            c.BackColor = Color.White;
            try
            {
                LifeTimeDiagramEditor.LifeTimeElement o = _lifeTimeObject as LifeTimeDiagramEditor.LifeTimeElement;
                o.Type = c.value;
                SetObject(_lifeTimeObject);

                ObjectChangedArgs objChangedArgs = new ObjectChangedArgs();
                objChangedArgs.NewColorsRequested = true;
                objChangedArgs.DiagramChanged = _allowDiagramChanging;
                objChangedArgs.ObjectChangedByPropertyGrid = true;
                ObjectChanged?.Invoke(_lifeTimeObject, objChangedArgs);
            }
            catch { c.BackColor = ErrorBackColor; }
        }
        private void CheckBoxChanged(object sender, EventArgs e)
        {
            CheckBox c = sender as CheckBox;
            c.BackColor = Color.White;

            if (_lifeTimeObject is LifeTimeDiagramEditor.LifeTimeElement) 
                CheckBoxChanged<LifeTimeDiagramEditor.LifeTimeElement>(c, _lifeTimeObject as LifeTimeDiagramEditor.LifeTimeElement);
            if (_lifeTimeObject is LifeTimeDiagramEditor.LifeTimeGroup) 
                CheckBoxChanged<LifeTimeDiagramEditor.LifeTimeGroup>(c, _lifeTimeObject as LifeTimeDiagramEditor.LifeTimeGroup);
            if (_lifeTimeObject is LifeTimeDiagramEditor.LifeTimeDiagramSettings) 
                CheckBoxChanged<LifeTimeDiagramEditor.LifeTimeDiagramSettings>(c, _lifeTimeObject as LifeTimeDiagramEditor.LifeTimeDiagramSettings);
        }
        public void CheckBoxChanged<T>(CheckBox c, T o)
        {
            Type t = typeof(T);
            try
            {
                t.GetProperty(c.Name).SetValue(o, c.Checked);
                SetObject(o as LifeTimeDiagramEditor.ILifeTimeObject);

                ObjectChangedArgs objChangedArgs = new ObjectChangedArgs();
                objChangedArgs.NewColorsRequested = true;
                objChangedArgs.DiagramChanged = _allowDiagramChanging;
                objChangedArgs.ObjectChangedByPropertyGrid = true;
                if (ObjectChanged != null) ObjectChanged(_lifeTimeObject, objChangedArgs);
            }
            catch { c.BackColor = ErrorBackColor; }
        }
        private void TextBoxChanged(object sender, EventArgs e)
        {
            AdvancedTextBox c = sender as AdvancedTextBox;
            c.TextBox.BackColor = Color.White;
            if (_lifeTimeObject is LifeTimeDiagramEditor.LifeTimeElement) TextBoxChanged<LifeTimeDiagramEditor.LifeTimeElement>(c, _lifeTimeObject as LifeTimeDiagramEditor.LifeTimeElement);
            if (_lifeTimeObject is LifeTimeDiagramEditor.LifeTimeGroup) TextBoxChanged<LifeTimeDiagramEditor.LifeTimeGroup>(c, _lifeTimeObject as LifeTimeDiagramEditor.LifeTimeGroup);
            if (_lifeTimeObject is LifeTimeDiagramEditor.LifeTimeDiagramSettings) TextBoxChanged<LifeTimeDiagramEditor.LifeTimeDiagramSettings>(c, _lifeTimeObject as LifeTimeDiagramEditor.LifeTimeDiagramSettings);
        }
        private void TextBoxIntChanged(object sender, EventArgs e)
        {
            AdvancedTextBox c = sender as AdvancedTextBox;
            c.BackColor = Color.White;

            if (_lifeTimeObject is LifeTimeDiagramEditor.LifeTimeElement) TextBoxChanged<LifeTimeDiagramEditor.LifeTimeElement>(c, _lifeTimeObject as LifeTimeDiagramEditor.LifeTimeElement);
            if (_lifeTimeObject is LifeTimeDiagramEditor.LifeTimeGroup) TextBoxChanged<LifeTimeDiagramEditor.LifeTimeGroup>(c, _lifeTimeObject as LifeTimeDiagramEditor.LifeTimeGroup);
            if (_lifeTimeObject is LifeTimeDiagramEditor.LifeTimeDiagramSettings) TextBoxChanged<LifeTimeDiagramEditor.LifeTimeDiagramSettings>(c, _lifeTimeObject as LifeTimeDiagramEditor.LifeTimeDiagramSettings);
        }
        private void TextBoxDoubleChanged(object sender, EventArgs e)
        {
            AdvancedTextBox c = sender as AdvancedTextBox;
            c.BackColor = Color.White;

            if (_lifeTimeObject is LifeTimeDiagramEditor.LifeTimeElement) TextBoxChanged<LifeTimeDiagramEditor.LifeTimeElement>(c, _lifeTimeObject as LifeTimeDiagramEditor.LifeTimeElement);
            if (_lifeTimeObject is LifeTimeDiagramEditor.LifeTimeGroup) TextBoxChanged<LifeTimeDiagramEditor.LifeTimeGroup>(c, _lifeTimeObject as LifeTimeDiagramEditor.LifeTimeGroup);
            if (_lifeTimeObject is LifeTimeDiagramEditor.LifeTimeDiagramSettings) TextBoxChanged<LifeTimeDiagramEditor.LifeTimeDiagramSettings>(c, _lifeTimeObject as LifeTimeDiagramEditor.LifeTimeDiagramSettings);
        }
        private void TextBoxChanged<T>(AdvancedTextBox c, T o)
        {
            Type t = typeof(T);
            try
            {
                if (t.GetProperty(c.Name).PropertyType == typeof(String))
                    t.GetProperty(c.Name).SetValue(o, c.Text);
                if (t.GetProperty(c.Name).PropertyType == typeof(Int32))
                    t.GetProperty(c.Name).SetValue(o, Convert.ToInt16(c.Text));
                if (t.GetProperty(c.Name).PropertyType == typeof(Double))
                    t.GetProperty(c.Name).SetValue(o, Convert.ToDouble(c.Text));

                SetObject(o as LifeTimeDiagramEditor.ILifeTimeObject);

                ObjectChangedArgs objChangedArgs = new ObjectChangedArgs();
                objChangedArgs.NewColorsRequested = true;
                objChangedArgs.DiagramChanged = _allowDiagramChanging;
                objChangedArgs.ObjectChangedByPropertyGrid = true;
                if (ObjectChanged != null) ObjectChanged(_lifeTimeObject, objChangedArgs);
            }
            catch { c.TextBox.BackColor = ErrorBackColor; }
        }
        private void DateTimeChanged(object sender, EventArgs e)
        {
            DateTimePicker c = sender as DateTimePicker;
            c.BackColor = Color.White;

            Type t = null;
            if (_lifeTimeObject is LifeTimeDiagramEditor.LifeTimeElement) t = typeof(LifeTimeDiagramEditor.LifeTimeElement);
            if (_lifeTimeObject is LifeTimeDiagramEditor.LifeTimeDiagramSettings) t = typeof(LifeTimeDiagramEditor.LifeTimeDiagramSettings);
            try
            {
                t.GetProperty(c.Name).SetValue(_lifeTimeObject, c.Value);
                SetObject(_lifeTimeObject);

                ObjectChangedArgs objChangedArgs = new ObjectChangedArgs();
                objChangedArgs.NewColorsRequested = true;
                objChangedArgs.DiagramChanged = _allowDiagramChanging;
                objChangedArgs.ObjectChangedByPropertyGrid = true;
                if (ObjectChanged != null) ObjectChanged(_lifeTimeObject, objChangedArgs);
            }
            catch { c.BackColor = ErrorBackColor; }
        }
        private void ColorChanged(object sender, EventArgs e)
        {
            ColorSelectorButton c = sender as ColorSelectorButton;

            if (_lifeTimeObject is LifeTimeDiagramEditor.LifeTimeElement) ColorChanged<LifeTimeDiagramEditor.LifeTimeElement>(c, _lifeTimeObject as LifeTimeDiagramEditor.LifeTimeElement);
            if (_lifeTimeObject is LifeTimeDiagramEditor.LifeTimeGroup) ColorChanged<LifeTimeDiagramEditor.LifeTimeGroup>(c, _lifeTimeObject as LifeTimeDiagramEditor.LifeTimeGroup);
            if (_lifeTimeObject is LifeTimeDiagramEditor.LifeTimeDiagramSettings) ColorChanged<LifeTimeDiagramEditor.LifeTimeDiagramSettings>(c, _lifeTimeObject as LifeTimeDiagramEditor.LifeTimeDiagramSettings);
        }
        private void ColorChanged<T>(ColorSelectorButton c, T o)
        {
            Type t = typeof(T);
            try
            {
                t.GetProperty(c.Name).SetValue(o, c.Value);
                SetObject(o as LifeTimeDiagramEditor.ILifeTimeObject);

                ObjectChangedArgs objChangedArgs = new ObjectChangedArgs();
                objChangedArgs.NewColorsRequested = true;
                objChangedArgs.DiagramChanged = _allowDiagramChanging;
                objChangedArgs.ObjectChangedByPropertyGrid = true;
                if (ObjectChanged != null) ObjectChanged(_lifeTimeObject, objChangedArgs);
            }
            catch { }
        }
        #endregion

        #region EventArgs Classes
        public class ObjectChangedArgs : EventArgs
        {
            public bool NewColorsRequested { get; set; }
            public bool DiagramChanged { get; set; }
            public bool ObjectChangedByPropertyGrid { get; set; }

            public ObjectChangedArgs()
            { }
        }
        #endregion

        #region LifeTimeToolbox Controls
        public class TypeSelectorBox : ComboBox
        {
            #region properties
            public LifeTimeDiagramEditor.LifeTimeElement.LifeTimeObjectType value { get; set; }
            #endregion

            #region constructor
            public TypeSelectorBox()
            {
                this.Items.Add(LifeTimeV3TextList.GetText("[10]")); //TimeSpan
                this.Items.Add(LifeTimeV3TextList.GetText("[11]")); //Event
                this.Items.Add(LifeTimeV3TextList.GetText("[12]")); //Marker
                this.Items.Add(LifeTimeV3TextList.GetText("[13]")); //Text
                this.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
                this.TextChanged += new EventHandler(SelectionChanged);
            }
            #endregion

            #region private methods
            private void SelectionChanged(object sender, EventArgs e)
            {
                if (this.Text == LifeTimeV3TextList.GetText("[10]")) value = LifeTimeDiagramEditor.LifeTimeElement.LifeTimeObjectType.TimeSpan;
                else if (this.Text == LifeTimeV3TextList.GetText("[11]")) value = LifeTimeDiagramEditor.LifeTimeElement.LifeTimeObjectType.Event;
                else if (this.Text == LifeTimeV3TextList.GetText("[12]")) value = LifeTimeDiagramEditor.LifeTimeElement.LifeTimeObjectType.Marker;
                else if (this.Text == LifeTimeV3TextList.GetText("[13]")) value = LifeTimeDiagramEditor.LifeTimeElement.LifeTimeObjectType.Text;
                else
                {
                    value = LifeTimeDiagramEditor.LifeTimeElement.LifeTimeObjectType.Event;
                    this.Text = LifeTimeV3TextList.GetText("[11]");
                }
            }
            #endregion
        }

        public class ColorSelectorButton : Button
        {
            #region properties
            public Color Value
            {
                get { return this.BackColor; }
                set { this.BackColor = value; }
            }
            #endregion

            #region constructor
            public ColorSelectorButton()
            {
                this.BackColor = Color.White;
                this.Width = 100;
                this.Text = LifeTimeV3TextList.GetText("[20]"); //Change Color
                this.Click += new EventHandler(ChangeColor);
            }
            #endregion

            #region private methods
            private void ChangeColor(object sender, EventArgs e)
            {
                ColorDialog d = new ColorDialog();
                d.Color = Value;
                d.ShowDialog();

                Value = d.Color;
            }
            #endregion
        }

        public class AdvancedTextBox : Panel
        {
            #region properties
            public override String Text
            {
                get { return TextBox.Text; }
                set { TextBox.Text = value; }
            }
            public override Color BackColor
            {
                get
                {
                    return TextBox.BackColor;
                }
                set
                {
                    TextBox.BackColor = value;
                }
            }
            public TextBox TextBox { get; set; }
            public Button Button { get; set; }
            #endregion

            #region fields
            private bool _isMultiLine;
            #endregion

            #region constructor
            public AdvancedTextBox()
            {
                _isMultiLine = false;
                Height = 22;
                TextBox = new TextBox();
                TextBox.Dock = DockStyle.Fill;
                Controls.Add(TextBox);
                TextBox.PreviewKeyDown += new PreviewKeyDownEventHandler(TakeOver);
            }
            #endregion

            #region public methods
            public void EnableMultiline()
            {
                _isMultiLine = true;
                TextBox.Multiline = true;                                
                Height = 90;

                Label hint = new Label();                
                hint.Text = LifeTimeV3TextList.GetText("[2000]");
                hint.Dock = DockStyle.Bottom;
                hint.BackColor = Color.LightYellow;
                hint.ForeColor = Color.DimGray;                                

                Controls.Add(hint);
            }
            #endregion

            #region private methods
            private void TakeOver(object sender, PreviewKeyDownEventArgs e)
            {
                if (_isMultiLine && e.KeyCode == Keys.Enter && e.Alt)
                {
                    ValueChanged?.Invoke(this, e);                    
                }
                else if (!_isMultiLine && e.KeyCode == Keys.Enter)
                    ValueChanged?.Invoke(this, e);
                
            }
            #endregion

            #region ValueChangedEvent
            public delegate void ValueChangedHandler(object sender, EventArgs e);
            public event ValueChangedHandler ValueChanged;
            #endregion

        }
        #endregion
    }

    /// <summary>
    /// Export Property Grid
    /// </summary>
    public class LifeTimeExportPNGPropertyGrid : TableLayoutPanel
    {
        #region Fields
        private LifeTimeDiagramEditor.LifeTimeExportSettings _exportSettings;
        private Color ErrorBackColor = Color.Orange;
        #endregion

        #region constructor
        public LifeTimeExportPNGPropertyGrid()
        {
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Show Properties by giving a new LifeTimeExportSettings object
        /// </summary>
        /// <param name="o"></param>
        public void SetExportSettings(LifeTimeDiagramEditor.LifeTimeExportSettings o)
        {
            _exportSettings = o;

            UpdateProperties();
        }

        /// <summary>
        /// Update Properties
        /// </summary>
        public void UpdateProperties()
        {
            this.Controls.Clear();

            int r = 0;
            AddExportToProperty(ref r);
            AddOverwriteTargetProperty(ref r);
            AddWidthHeightProperty(ref r);
            AddExportButton(ref r);
            AddEmptyLabel(ref r);
        }
        #endregion

        #region Private Methods
        private void AddExportToProperty(ref int r)
        {
            Label l = new Label();
            l.Text = LifeTimeV3TextList.GetText("[22]"); //Target
            l.TextAlign = ContentAlignment.MiddleRight;
            this.Controls.Add(l, 0, r);

            TextBox t = new TextBox();
            t.Name = "FileName";
            t.Dock = DockStyle.Fill;
            t.Text = _exportSettings.FileName;
            this.Controls.Add(t, 1, r);
            r++;
            Button b = new Button();
            b.Dock = DockStyle.Right;
            b.Text = "...";

            b.Click += new EventHandler(ChooseTarget);

            this.Controls.Add(b, 1, r);
            r++;
        }

        private void AddOverwriteTargetProperty(ref int r)
        {
            CheckBox c = new CheckBox();
            c.Dock = DockStyle.Fill;
            c.Name = "Overwrite";
            c.Text = LifeTimeV3TextList.GetText("[23]"); //Overwrite
            c.Checked = _exportSettings.OverwriteExisting;
            this.Controls.Add(c, 1, r);
            r++;
        }

        private void AddWidthHeightProperty(ref int r)
        {
            Label l = new Label();
            l.Text = LifeTimeV3TextList.GetText("[24]"); //Width
            l.TextAlign = ContentAlignment.MiddleRight;
            this.Controls.Add(l, 0, r);

            TextBox t = new TextBox();
            t.Dock = DockStyle.Fill;
            t.Name = "Width";
            t.Text = Convert.ToString(_exportSettings.Width);
            this.Controls.Add(t, 1, r);
            r++;

            l = new Label();
            l.Text = LifeTimeV3TextList.GetText("[25]"); //Height
            l.TextAlign = ContentAlignment.MiddleRight;
            this.Controls.Add(l, 0, r);

            t = new TextBox();
            t.Dock = DockStyle.Fill;
            t.Name = "Height";
            t.Text = Convert.ToString(_exportSettings.Height);
            this.Controls.Add(t, 1, r);
            r++;
        }

        private void AddExportButton(ref int r)
        {
            Button b = new Button();
            b.Dock = DockStyle.Right;
            b.Text = LifeTimeV3TextList.GetText("[26]"); //Export

            b.Click += new EventHandler(Export);

            this.Controls.Add(b, 1, r);
            r++;
        }

        private void AddEmptyLabel(ref int r)
        {
            Label l = new Label();
            l.TextAlign = ContentAlignment.MiddleRight;
            this.Controls.Add(l, 0, r);
            r++;
        }
        #endregion

        #region Events
        private void ChooseTarget(object sender, EventArgs e)
        {
            SaveFileDialog d = new SaveFileDialog();
            d.Filter = string.Format("*.png|*.png");
            d.RestoreDirectory = true;
            d.ShowDialog();

            if (d.FileName != "") _exportSettings.FileName = d.FileName;

            if (File.Exists(d.FileName)) _exportSettings.OverwriteExisting = true;

            UpdateProperties();
        }

        private void Export(object sender, EventArgs e)
        {
            _exportSettings.FileName = this.Controls["FileName"].Text;
            _exportSettings.OverwriteExisting = (this.Controls["Overwrite"] as CheckBox).Checked;

            this.Controls["Width"].BackColor = Color.White;
            this.Controls["Height"].BackColor = Color.White;

            try
            {
                _exportSettings.Width = Convert.ToInt16(this.Controls["Width"].Text);
            }
            catch
            {
                this.Controls["Width"].BackColor = ErrorBackColor;
                return;
            }

            try
            {
                _exportSettings.Height = Convert.ToInt16(this.Controls["Height"].Text);
            }
            catch
            {
                this.Controls["Height"].BackColor = ErrorBackColor;
                return;
            }

            UpdateProperties();

            if (File.Exists(_exportSettings.FileName) && !_exportSettings.OverwriteExisting)
            {
                DialogResult r = MessageBox.Show(LifeTimeV3TextList.GetText("[302]"), "Export", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (r == DialogResult.No) return;
            }

            ExportButtonClick(this, e);
        }
        #endregion

        #region EventHandler
        public delegate void ExportButtonClicked(object sender, EventArgs e);
        public event ExportButtonClicked ExportButtonClick;
        #endregion
    }
}