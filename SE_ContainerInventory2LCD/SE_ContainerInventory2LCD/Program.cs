using Sandbox.ModAPI.Ingame;
using System.Linq;
using VRage.Game.ModAPI.Ingame;
using VRage.Network;
using VRage.ObjectBuilders;
using static System.Net.Mime.MediaTypeNames;

namespace SE_ContainerInventory2LCD
{
    /// <summary>
    /// Container Inventory 2 LCD
    /// Fetches all connected blocks with an inventory and shows their summed up amount in corresponding LCD screens.
    /// </summary>
    public sealed class Program : MyGridProgram
    {
        private const int INITAL_VALUE = 0;

        private const string LEGACY_TYPE_PREFIX = "MyObjectBuilder_";
        private const string LCD_AVAILABLE_ITEMS_SUFFIX = "[available]";
        private const string LCD_UNAVAILABLE_ITEMS_SUFFIX = "[out]";

        #region Variables

        private List<IMyTerminalBlock> _terminalConnectedInventories = new List<IMyTerminalBlock>();
        private List<IMyTerminalBlock> _terminalConnectedLCDs = new List<IMyTerminalBlock>();

        private Dictionary<string, double> _currentItems = new Dictionary<string, double>()
        {
            { ComponentItem.BulletproofGlass.SubtypeId, INITAL_VALUE },
            { ComponentItem.Computer.SubtypeId, INITAL_VALUE },
            { ComponentItem.Construction.SubtypeId, INITAL_VALUE },
            { ComponentItem.Detector.SubtypeId, INITAL_VALUE },
            { ComponentItem.Display.SubtypeId, INITAL_VALUE },
            { ComponentItem.Explosives.SubtypeId, INITAL_VALUE },
            { ComponentItem.Girder.SubtypeId, INITAL_VALUE },
            { ComponentItem.Gravity.SubtypeId, INITAL_VALUE },
            { ComponentItem.InteriorPlate.SubtypeId, INITAL_VALUE },
            { ComponentItem.Medical.SubtypeId, INITAL_VALUE },
            { ComponentItem.MetalGrid.SubtypeId, INITAL_VALUE },
            { ComponentItem.Motor.SubtypeId, INITAL_VALUE },
            { ComponentItem.PowerCell.SubtypeId, INITAL_VALUE },
            { ComponentItem.RadioCommunication.SubtypeId, INITAL_VALUE },
            { ComponentItem.Reactor.SubtypeId, INITAL_VALUE },
            { ComponentItem.SolarCell.SubtypeId, INITAL_VALUE },
            { ComponentItem.SteelPlate.SubtypeId, INITAL_VALUE },
            { ComponentItem.SteelTubeLarge.SubtypeId, INITAL_VALUE },
            { ComponentItem.SteelTubeSmall.SubtypeId, INITAL_VALUE },
            { ComponentItem.Thruster.SubtypeId, INITAL_VALUE },
        };

        #endregion

        #region Helper

        private void ResetAvailableItemCount()
        {
            foreach (string key in _currentItems.Keys.ToList())
            {
                _currentItems[key] = INITAL_VALUE;
            }
        }

        private string FormatAmount(double value)
        {
            string unit = "";

            if (value >= 1000000)
            {
                unit = "M";
                value /= 1000000;
            }
            else if (value >= 1000)
            {
                unit = "k";
                value /= 1000;
            }

            return "" + (int)value + unit;
        }

        #endregion

        #region Script Entry Point

        /// <summary>
        /// Initial settings.
        /// </summary>
        /// <remarks>
        /// Serves as a constructor to intialise objects. Executed once at the start, after the script has been compiled.
        /// </remarks>
        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
        }

        /// <summary>
        /// Script main entry.
        /// </summary>
        /// <remarks>
        /// Runs every time the script is executed by the player. This is the only method that is mandatory for the script to run.
        /// </remarks>
        /// <param name="args"></param>
        /// <param name="updateSource"></param>
        public void Main(string args, UpdateType updateSource)
        {
            FetchConnectedInventories();
            FetchConnectedLCDs();

            UpdateAvailableItems();

            PrintToLCD();
        }

        #endregion

        #region Script Methods

        /// <summary>
        /// Fetch all blocks which are currently connected with the terminal block and own itself an inventory.
        /// </summary>
        public void FetchConnectedInventories()
        {
            _terminalConnectedInventories.Clear();

            GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(_terminalConnectedInventories, x => x.HasInventory);
        }

        /// <summary>
        /// Fetch all blocks of <see cref="IMyTextPanel"/> type (Text Panel, LCD, Wide LCD).
        /// </summary>
        public void FetchConnectedLCDs()
        {
            _terminalConnectedLCDs.Clear();

            GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(_terminalConnectedLCDs, x => x is IMyTextPanel);
        }

        /// <summary>
        /// Update the item amount list.
        /// </summary>
        public void UpdateAvailableItems()
        {
            ResetAvailableItemCount();

            var items = new List<MyInventoryItem>();

            string itemKey;
            double itemAmount;

            foreach (IMyTerminalBlock block in _terminalConnectedInventories)
            {
                for (int i = 0; i < block.InventoryCount; i++)
                {
                    block.GetInventory(i).GetItems(items);
                }

                foreach (MyInventoryItem item in items)
                {
                    if (item.Type.TypeId == ComponentItem.TypeId)
                    {
                        itemKey = item.Type.SubtypeId;

                        itemAmount = (double)item.Amount.RawValue / 1000000d;

                        if (_currentItems.ContainsKey(itemKey))
                        {
                            _currentItems[itemKey] += itemAmount;
                        }
                    }
                }

                items.Clear();
            }
        }

        /// <summary>
        /// Prints items to LCDs.
        /// </summary>
        public void PrintToLCD()
        {
            if (_terminalConnectedLCDs.Count > 0)
            {
                IMyTextPanel panel;
                IMyTextSurface textSurface;

                IOrderedEnumerable<KeyValuePair<string, double>> sortedDict = from entry in _currentItems orderby entry.Value descending select entry;
                IEnumerable<KeyValuePair<string, double>> availableItems = sortedDict.Where(x => x.Value > 0);
                IEnumerable<KeyValuePair<string, double>> unavailableItems = sortedDict.Where(x => x.Value == 0);

                string text = "";

                foreach (IMyTerminalBlock terminalConnectedLCD in _terminalConnectedLCDs)
                {
                    panel = (IMyTextPanel)terminalConnectedLCD;

                    textSurface = (IMyTextSurface)terminalConnectedLCD;

                    //Available
                    if (panel.CustomName.EndsWith(LCD_AVAILABLE_ITEMS_SUFFIX))
                    {
                        textSurface.ContentType = VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;

                        foreach (var item in availableItems)
                        {
                            text += item.Key + " " + FormatAmount(item.Value) + "\r\n";
                        }

                        textSurface.WriteText(text);

                        text = "";
                    }

                    // Unavailable
                    if (panel.CustomName.EndsWith(LCD_UNAVAILABLE_ITEMS_SUFFIX))
                    {
                        textSurface.ContentType = VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;

                        foreach (var item in unavailableItems)
                        {
                            text += item.Key + " " + FormatAmount(item.Value) + "\r\n";
                        }

                        textSurface.WriteText(text);

                        text = "";
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Type and name of component items.
        /// </summary>
        public static class ComponentItem
        {
            /// <summary>
            /// Prefix when using MyInventoryItem.Type.TypeId
            /// </summary>
            public const string TypeId = LEGACY_TYPE_PREFIX + "Component";

            public static Item BulletproofGlass     = new Item() { SubtypeId = "BulletproofGlass", DisplayName = "Bulletproof Glass" };
            public static Item Computer             = new Item() { SubtypeId = "Computer", DisplayName = "Computer" };
            public static Item Construction         = new Item() { SubtypeId = "Construction", DisplayName = "Construction" };
            public static Item Detector             = new Item() { SubtypeId = "Detector", DisplayName = "Detector" };
            public static Item Display              = new Item() { SubtypeId = "Display", DisplayName = "Display" };
            public static Item Explosives           = new Item() { SubtypeId = "Explosives", DisplayName = "Explosives" };
            public static Item Girder               = new Item() { SubtypeId = "Girder", DisplayName = "Girder" };
            public static Item Gravity              = new Item() { SubtypeId = "Gravity", DisplayName = "Gravity" };
            public static Item InteriorPlate        = new Item() { SubtypeId = "InteriorPlate", DisplayName = "Interior Plate" };
            public static Item Medical              = new Item() { SubtypeId = "Medical", DisplayName = "Medical" };
            public static Item MetalGrid            = new Item() { SubtypeId = "MetalGrid", DisplayName = "Metal Grid" };
            public static Item Motor                = new Item() { SubtypeId = "Motor", DisplayName = "Motor" };
            public static Item PowerCell            = new Item() { SubtypeId = "PowerCell", DisplayName = "Power Cell" };
            public static Item RadioCommunication   = new Item() { SubtypeId = "RadioCommunication", DisplayName = "Radio Communication" };
            public static Item Reactor              = new Item() { SubtypeId = "Reactor", DisplayName = "Reactor" };
            public static Item SolarCell            = new Item() { SubtypeId = "SolarCell", DisplayName = "Solar Cell" };
            public static Item SteelPlate           = new Item() { SubtypeId = "SteelPlate", DisplayName = "Steel Plate" };
            public static Item SteelTubeLarge       = new Item() { SubtypeId = "LargeTube", DisplayName = "Large Tube" };
            public static Item SteelTubeSmall       = new Item() { SubtypeId = "SmallTube", DisplayName = "Small Tube" };
            public static Item Thruster             = new Item() { SubtypeId = "Thrust", DisplayName = "Thruster" };
            public static Item SuperConductor       = new Item() { SubtypeId = "Superconductor", DisplayName = "Super Conductor" };
        }

        public class Item
        {
            public string SubtypeId { get; set; } = "";
            public string DisplayName { get; set; } = "";
        }
    }
}
