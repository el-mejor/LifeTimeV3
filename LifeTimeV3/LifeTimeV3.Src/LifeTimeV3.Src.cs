using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LifeTimeV3.Src
{
    class LifeTimeV3TextList
    {   
        public enum Language { DE, EN }

        #region German
        string[] ListDE = {
                              "[1]", "Aktiviert", //Texts
                              "[10]", "Zeitspanne",
                              "[11]", "Zeitpunkt",
                              "[12]", "Marker",
                              "[20]", "Farbe ändern",
                              "[21]", "Suchen",
                              "[22]", "Ziel",
                              "[23]", "Ziel überschreiben",
                              "[24]", "Breite",
                              "[25]", "Höhe",
                              "[26]", "Exportieren",
                              "[100]", "Kein Element ausgewählt.",
                              "[101]", "Root",
                              "[102]", "Neues Element",
                              "[103]", "Neue Gruppe",
                              "[200]", "Element hinzufügen",
                              "[201]", "Gruppe hinzufügen",
                              "[202]", "Verschieben...",
                              "[203]", "Löschen",
                              "[204]", "Kopieren",
                              "[205]", "Ausschneiden",
                              "[206]", "Einfügen",
                              "[207]", "In den Vordergrund",
                              "[208]", "Nach vorne",
                              "[209]", "Nach hinten",
                              "[210]", "In den Hintergrund",
                              "[211]", "Alles ausklappen",
                              "[212]", "Alles einklappen",
                              "[213]", "Werkzeugkasten",
                              "[214]", "Element",
                              "[215]", "Einstellungen",
                              "[216]", "Export",
                              "[217]", "Nicht gespeichert.",                              

                              "[300]", "Soll das ausgewählte Element \"{0}\" wirklich gelöscht werden?",
                              "[301]", "Soll die ausgewählte Gruppe \"{0}\" wirklich gelöscht werden? Alle darin befindlichen Elemente und Gruppen gehen dabei verloren!",
                              "[302]", "Die Zieldatei existiert bereits. Soll sie überschrieben werden?",
                              "[303]", "Element: \"{0}\" - vor {1} jahren ({2} Tage), dauerte {3} Jahre ({4} Tage)",

                              "Name","Name", //LifeTimeObject Properties
                              "Type","Element Typ",
                              "Path","Gruppe",
                              "Begin", "Beginn",
                              "End", "Ende",
                              "BeginsToday", "Beginnt heute",
                              "EndsToday", "Endet heute",
                              "LineDeviation", "Verschiebung",
                              "BaseColor", "Basis Farbe",
                              "Color", "Aktuelle Farbe",
                              "FixedColor", "Festgelegte Farbe",
                              "Opacity", "Transparenz",
                              "GetRandomColor", "Zufällige Farbe",
                              "Row", "Row",
                              "Size", "Größe",
                              "TextPosX", "Textposition X",
                              "TextPosY", "Textposition Y",

                              "GroupColor", "Gruppenfarbe", //LifeTimeGroup Properties
                              "OwnColor", "Eigene Farbe",
  
                              "GroupHeight", "Gruppenabstand", //LifeTimeSettings Properties
                              "BlockHeight", "Blockhöhe",
                              "Width", "Breite",
                              "Height", "Höhe",
                              "GroupAreaWidth", "Gruppenbereichsbreite",
                              "Border", "Rand",
                              "BackColor", "Hintergrundfarbe",
                              "DrawShadows", "Schatten",
                              "LabelColor", "Textfarbe"
                          };
        #endregion

        #region English
        string[] ListEN = {
                              "[1]", "Enabled", //Texts
                              "[10]", "Timespan",
                              "[11]", "Event",
                              "[12]", "Marker",
                              "[20]", "Change Color",
                              "[21]", "Search",
                              "[22]", "Target",
                              "[23]", "Overwrite target",
                              "[24]", "Width",
                              "[25]", "Height",
                              "[26]", "Export",
                              "[100]","No object selected.",  
                              "[101]","Root",
                              "[102]", "New element",
                              "[103]", "New group",
                              "[200]", "Add new element",
                              "[201]", "Add new group",
                              "[202]", "Move to ...",
                              "[203]", "Delete",
                              "[204]", "Copy",
                              "[205]", "Cut",
                              "[206]", "Paste",
                              "[207]", "Bring to the front",
                              "[208]", "Move to the front",
                              "[209]", "Move to the back",
                              "[210]", "Bring to the back",
                              "[211]", "Expand all",
                              "[212]", "Collapse all",
                              "[213]", "Toolbox",
                              "[214]", "Element",
                              "[215]", "Settings",
                              "[216]", "Export",
                              "[217]", "Unsaved changes.",

                              "[300]", "Would you really delete the element \"{0}\"?",
                              "[301]", "Would you really delete the group \"{0}\"? All containing elements and groups will be lost!",
                              "[302]", "The target file already exists. Would you overwrite it?",
                              "[303]", "Element: \"{0}\" - {1} years ({2} days) ago, lasted for {3} years ({4} days)",

                              "Name","Name", //LifeTimeObject Properties
                              "Type","Object Type",
                              "Path","Group",
                              "Begin", "Begin",
                              "End", "End",
                              "BeginsToday", "Begins today",
                              "EndsToday", "Ends today",
                              "LineDeviation", "Shift",
                              "BaseColor", "Basic Color",
                              "Color", "Current Color",
                              "FixedColor", "Fixed Color",
                              "Opacity", "Opacity",
                              "GetRandomColor", "Gets random Color",
                              "Row", "Row",
                              "Size", "Size",
                              "TextPosX", "Textposition X",
                              "TextPosY", "Textposition Y",

                              "GroupColor", "Group Color", //LifeTimeGroup Properties
                              "OwnColor", "Own Color",           
                              
                              "GroupHeight", "Groupspacing", //LifeTimeSettings Properties
                              "BlockHeight", "Blockheight",
                              "Width", "Width",
                              "Height", "Height",
                              "GroupAreaWidth", "Group area width",
                              "Border", "Border",
                              "BackColor", "Backcolor",
                              "DrawShadows", "Shadows",
                              "LabelColor", "Labelcolor"
                          };
        #endregion

        public LifeTimeV3TextList()
        {
            Language lang = Language.DE;
            if (Properties.Settings.Default.Language == "EN")
                lang = Language.EN;
            
            if (lang == Language.DE) TextList = LoadTextList(ListDE);
            if (lang == Language.EN) TextList = LoadTextList(ListEN);
        }

        #region Properties
        public Dictionary<string, string> TextList;
        #endregion
        
        #region public methods
        public static string GetText(string id)
        {
            LifeTimeV3TextList textList = new LifeTimeV3TextList();
            String retString = "TextDummy";
            try { textList.TextList.TryGetValue(id, out retString); }
            catch { retString = "TextNotFound"; }
            return retString;            
        }
        #endregion

        #region private Methods
        private Dictionary<string, string> LoadTextList(string[] list)
        {
            Dictionary<string, string> textlist = new Dictionary<string, string>();

            for (int i = 0; i < list.Length; i+=2)
            {
                textlist.Add(list[i], list[i + 1]);
            }

            return textlist;
        }
        #endregion
    }
}
