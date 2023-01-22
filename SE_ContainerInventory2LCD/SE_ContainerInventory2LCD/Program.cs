using Sandbox.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame;
using VRage.Network;
using VRage.ObjectBuilders;

namespace SE_ContainerInventory2LCD
{
    /// <summary>
    /// Container Inventory 2 LCD
    /// Fetches all connected blocks with an inventory and shows their summed up amount in corresponding LCD screens.
    /// </summary>
    public sealed class Program : MyGridProgram
    {
        const int INITAL_VALUE = 0;

        #region Variables

        private List<IMyTerminalBlock> _terminalConnectedInventories = new List<IMyTerminalBlock>();

        private Dictionary<string, double> _currentAvailableItems = new Dictionary<string, double>()
        {
            { ComponentItem.BulletproofGlass.SubtypeId, INITAL_VALUE },
            { ComponentItem.Computer.SubtypeId, INITAL_VALUE },
            { ComponentItem.ConstructioonComponent.SubtypeId, INITAL_VALUE },
            { ComponentItem.DetectorComponent.SubtypeId, INITAL_VALUE },
            { ComponentItem.Display.SubtypeId, INITAL_VALUE },
            { ComponentItem.Explosives.SubtypeId, INITAL_VALUE },
            { ComponentItem.Girder.SubtypeId, INITAL_VALUE },
            { ComponentItem.GravityComponent.SubtypeId, INITAL_VALUE },
            { ComponentItem.InteriorPlate.SubtypeId, INITAL_VALUE },
            { ComponentItem.MedicalComponents.SubtypeId, INITAL_VALUE },
            { ComponentItem.MetalGrid.SubtypeId, INITAL_VALUE },
            { ComponentItem.Motor.SubtypeId, INITAL_VALUE },
            { ComponentItem.PowerCell.SubtypeId, INITAL_VALUE },
            { ComponentItem.RadioCommunicationComponent.SubtypeId, INITAL_VALUE },
            { ComponentItem.ReactorComponents.SubtypeId, INITAL_VALUE },
            { ComponentItem.SolarCell.SubtypeId, INITAL_VALUE },
            { ComponentItem.SteelPlate.SubtypeId, INITAL_VALUE },
            { ComponentItem.SteelTubeLarge.SubtypeId, INITAL_VALUE },
            { ComponentItem.SteelTubeSmall.SubtypeId, INITAL_VALUE },
            { ComponentItem.ThrusterComponents.SubtypeId, INITAL_VALUE },
        };

        #endregion

        #region Helper

        private void ResetAvailableItemCount()
        {
            foreach (string key in _currentAvailableItems.Keys.ToList())
            {
                _currentAvailableItems[key] = INITAL_VALUE;
            }
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
            FetchAvailableInventories();

            UpdateAvailableItems();

            // TODO initialize all LCD panels
        }

        #endregion

        #region Script Methods

        /// <summary>
        /// Fetch all blocks which are currently connected with the terminal block and own itself an inventory.
        /// </summary>
        public void FetchAvailableInventories()
        {
            _terminalConnectedInventories.Clear();

            GridTerminalSystem.GetBlocksOfType(_terminalConnectedInventories, x => x.HasInventory);
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
                        itemKey = item.Type.SubtypeId; // e. g. "BulletproofGlass"
                        itemAmount = (double)item.Amount.RawValue / 1000000d;

                        if (_currentAvailableItems.ContainsKey(itemKey))
                        {
                            _currentAvailableItems[itemKey] = itemAmount;
                        }
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
            public const string TypeId = MyObjectBuilderType.LEGACY_TYPE_PREFIX + "Component";

            public static Item BulletproofGlass             = new Item() { SubtypeId = "BulletproofGlass",              DisplayName = "Bulletproof Glass" };
            public static Item Computer                     = new Item() { SubtypeId = "Computer",                      DisplayName = "Computer" };
            public static Item ConstructioonComponent       = new Item() { SubtypeId = "ConstructioonComponent",        DisplayName = "Construction Component" };
            public static Item DetectorComponent            = new Item() { SubtypeId = "DetectorComponent",             DisplayName = "Detector Component" };
            public static Item Display                      = new Item() { SubtypeId = "Display",                       DisplayName = "Display" };
            public static Item Explosives                   = new Item() { SubtypeId = "Explosives",                    DisplayName = "Explosives" };
            public static Item Girder                       = new Item() { SubtypeId = "Girder",                        DisplayName = "Girder" };
            public static Item GravityComponent             = new Item() { SubtypeId = "GravityComponent",              DisplayName = "Gravity Component" };
            public static Item InteriorPlate                = new Item() { SubtypeId = "InteriorPlate",                 DisplayName = "Interior Plate" };
            public static Item MedicalComponents            = new Item() { SubtypeId = "MedicalComponents",             DisplayName = "Medical Components" };
            public static Item MetalGrid                    = new Item() { SubtypeId = "MetalGrid",                     DisplayName = "Metal Grid" };
            public static Item Motor                        = new Item() { SubtypeId = "Motor",                         DisplayName = "Motor" };
            public static Item PowerCell                    = new Item() { SubtypeId = "PowerCell",                     DisplayName = "Power Cell" };
            public static Item RadioCommunicationComponent  = new Item() { SubtypeId = "RadioCommunicationComponent",   DisplayName = "Radio Communication Component" };
            public static Item ReactorComponents            = new Item() { SubtypeId = "ReactorComponents",             DisplayName = "Reactor Components" };
            public static Item SolarCell                    = new Item() { SubtypeId = "SolarCell",                     DisplayName = "Solar Cell" };
            public static Item SteelPlate                   = new Item() { SubtypeId = "SteelPlate",                    DisplayName = "Steel Plate" };
            public static Item SteelTubeLarge               = new Item() { SubtypeId = "SteelTubeLarge",                DisplayName = "Steel Tube Large" };
            public static Item SteelTubeSmall               = new Item() { SubtypeId = "SteelTubeSmall",                DisplayName = "Steel Tube Small" };
            public static Item ThrusterComponents           = new Item() { SubtypeId = "ThrusterComponents",            DisplayName = "Thruster Components" };
        }

        public class Item
        {
            public string SubtypeId { get; set; } = "";
            public string DisplayName { get; set; } = "";
        }
    }
}
