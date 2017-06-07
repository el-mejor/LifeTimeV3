using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LifeTimeV3.Src
{
    class LifeTimeV3TextList
    {   
        // Add more language codes to this enum
        public enum Language { DE, EN }

        //Text lists
        //German
        #region German
        string[] ListDE = {
                              "[1]", "Aktiviert", //Texts
                              "[10]", "Zeitspanne",
                              "[11]", "Zeitpunkt",
                              "[12]", "Marker",
                              "[13]", "Text",
                              "[20]", "Farbe ändern",
                              "[21]", "Suchen",
                              "[22]", "Ziel",
                              "[23]", "Ziel überschreiben",
                              "[24]", "Breite",
                              "[25]", "Höhe",
                              "[26]", "Exportieren",
                              "[000]", "DATEI",
                              "[001]", "Neu...",
                              "[002]", "Öffnen...",
                              "[003]", "Speichern",
                              "[004]", "Speichern als...",
                              "[005]", "Beenden",
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
                              "[218]", "Periodisch vervielfältigen",
                              "[219]", "Vorheriges",
                              "[220]", "Nächstes",
                              "[221]", "nichts gefunden",
                              "[222]", "Ergebnis {0} von {1}",
                              "[223]", "Switch to english",
                              "[224]", "To switch language LifeTimeV3 needs to be closed.",
                              "[225]", "Über...",
                              "[226]", "Hilfsline",

                              "[300]", "Soll das ausgewählte Element \"{0}\" wirklich gelöscht werden?",
                              "[301]", "Soll die ausgewählte Gruppe \"{0}\" wirklich gelöscht werden? Alle darin befindlichen Elemente und Gruppen gehen dabei verloren!",
                              "[302]", "Die Zieldatei existiert bereits. Soll sie überschrieben werden?",
                              "[303]", "Element: \"{0}\" - vor {1} jahren ({2} Tage), dauerte {3} Jahre ({4} Tage)",

                              "[400]", "LifeTimeV3 - Element periodisch vervielfältigen",
                              "[401]", "Kopie erstellen alle",
                              "[402]", "Anzahl",
                              "[403]", "Bis zu Datum",
                              "[404]", "Tage",
                              "[405]", "Monate",
                              "[406]", "Jahre",

                              "[1000]", "Klicken Sie hier um die Toolbox zu öffnen um Elemente hinzuzufügen oder zu bearbeiten.",
                              "[1001]", "Legen Sie in der Toolbox zunächst eine Gruppe an (Rechtsklick auf Root).",
                              "[1002]", "Legen Sie in der Toolbox ein Element an (Rechtsklick auf eine Gruppe).",
                              "[2000]", "Übernehmen mit Alt+Enter",

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
                              "TextInBox", "Rahmen",
                              "Text", "Text",
                              "Locked", "Fixiert",
                              "None", "Ohne",
                              "Left", "Links",
                              "Center", "Zentriert",
                              "Right", "Rechts",
                              "Top", "Oben",
                              "Middle", "Mitte",
                              "Bottom", "Unten",
                              "HorizontallyBonding", "Ausrichtung H",
                              "VerticallyBonding", "Ausrichtung V",


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

        //English
        #region English
        string[] ListEN = {
                              "[1]", "Enabled", //Texts
                              "[10]", "Timespan",
                              "[11]", "Event",
                              "[12]", "Marker",
                              "[13]", "Text",
                              "[20]", "Change Color",
                              "[21]", "Search",
                              "[22]", "Target",
                              "[23]", "Overwrite target",
                              "[24]", "Width",
                              "[25]", "Height",
                              "[26]", "Export",
                              "[000]", "FILE",
                              "[001]", "New...",
                              "[002]", "Open...",
                              "[003]", "Save",
                              "[004]", "Save as...",
                              "[005]", "Close",
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
                              "[218]", "Multiply periodically",
                              "[219]", "Previous",
                              "[220]", "Next",
                              "[221]", "not found",
                              "[222]", "Result {0} of {1}",
                              "[223]", "Switch to german",                              
                              "[224]", "To switch language LifeTimeV3 needs to be closed.",
                              "[225]", "About...",
                              "[226]", "Referenceline",


                              "[300]", "Would you really delete the element \"{0}\"?",
                              "[301]", "Would you really delete the group \"{0}\"? All containing elements and groups will be lost!",
                              "[302]", "The target file already exists. Would you overwrite it?",
                              "[303]", "Element: \"{0}\" - {1} years ({2} days) ago, lasted for {3} years ({4} days)",

                              "[400]", "LifeTimeV3 - Add periodic copies of element",
                              "[401]", "Add copy every",
                              "[402]", "Ammount",
                              "[403]", "To date",
                              "[404]", "days",
                              "[405]", "month",
                              "[406]", "years",

                              "[1000]", "Click here to open the toolbox for adding and editing elements.",                              
                              "[1001]", "Use the toolbox to add a group (right click on the root folder).",
                              "[1002]", "Use the toolbox to add an element (right click on a group folder).",
                              "[2000]", "Alt+Enter to overtake",                              

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
                              "TextInBox", "Box",
                              "Text", "Text",
                              "Locked", "Locked",
                              "None", "None",
                              "Left", "Left",
                              "Center", "Center",
                              "Right", "Right",
                              "Top", "Top",
                              "Middle", "Middle",
                              "Bottom", "Bottom",
                              "HorizontallyBonding", "Bond hor.",
                              "VerticallyBonding", "Bon vert.",


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

        //Add more text lists here

        public LifeTimeV3TextList()
        {
            Language lang = Language.DE;
            if (Properties.Settings.Default.Language == "EN")
                lang = Language.EN;
            
            if (lang == Language.DE) TextList = LoadTextList(ListDE);
            if (lang == Language.EN) TextList = LoadTextList(ListEN);
            // Add new languagecode and corresponding text list here
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
