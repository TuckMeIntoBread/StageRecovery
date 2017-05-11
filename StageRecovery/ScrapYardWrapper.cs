﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

//TODO: Change namespace to your mod's namespace
namespace StageRecovery
{
    //DO NOT CHANGE ANYTHING BELOW THIS LINE
    public sealed class ScrapYardWrapper
    {
        private static bool? available;
        private static Type SYType;
        private static object _instance;

        /// <summary>
        /// The strictness of comparing two parts for equivalency
        /// </summary>
        public enum ComparisonStrength
        {
            /// <summary>
            /// Equivalent if their names match
            /// </summary>
            NAME,
            /// <summary>
            /// EqualEquivalent if name and dry cost match
            /// </summary>
            COSTS,
            /// <summary>
            /// Equaivalent if name, dry cost, and Modules (except ModuleSYPartTracker) match
            /// </summary>
            MODULES,
            /// <summary>
            /// Equivalent if name, dry cost, Modules, and TimesRecovered match
            /// </summary>
            TRACKER,
            /// <summary>
            /// Equivalent if name, dry cost, Modules, TimesRecovered and IDs match
            /// </summary>
            STRICT
        }

        /// <summary>
        /// True if ScrapYard is available, false if not
        /// </summary>
        public static bool Available
        {
            get
            {
                if (available == null)
                {
                    SYType = AssemblyLoader.loadedAssemblies
                        .Select(a => a.assembly.GetExportedTypes())
                        .SelectMany(t => t)
                        .FirstOrDefault(t => t.FullName == "ScrapYard.APIManager");
                    available = SYType != null;
                }
                return available.GetValueOrDefault();
            }
        }

        /// <summary>
        /// Removes inventory parts, refunds funds, marks it as tracked
        /// </summary>
        /// <param name="parts">The vessel as a List of Parts</param>
        /// <returns>True if processed, false otherwise</returns>
        public static bool ProcessVessel(IEnumerable<Part> parts)
        {
            if (!Available)
            {
                return false;
            }
            return (bool)invokeMethod("ProcessVessel_Parts", parts);
        }

        /// <summary>
        /// Removes inventory parts, refunds funds, marks it as tracked
        /// </summary>
        /// <param name="parts">The vessel as a List of part ConfigNodes</param>
        /// <returns>True if processed, false otherwise</returns>
        public static bool ProcessVessel(IEnumerable<ConfigNode> parts)
        {
            if (!Available)
            {
                return false;
            }
            return (bool)invokeMethod("ProcessVessel_Nodes", parts);
        }

        /// <summary>
        /// Adds a list of parts to the Inventory
        /// </summary>
        /// <param name="parts">The list of parts to add</param>
        /// <param name="incrementRecovery">If true, increments the number of recoveries in the tracker</param>
        public static void AddPartsToInventory(IEnumerable<Part> parts, bool incrementRecovery)
        {
            if (Available)
            {
                invokeMethod("AddPartsToInventory_Parts", parts, incrementRecovery);
            }
        }

        /// <summary>
        /// Adds a list of parts to the Inventory
        /// </summary>
        /// <param name="parts">The list of parts to add</param>
        /// <param name="incrementRecovery">If true, increments the number of recoveries in the tracker</param>
        public static void AddPartsToInventory(IEnumerable<ConfigNode> parts, bool incrementRecovery)
        {
            if (Available)
            {
                invokeMethod("AddPartsToInventory_Nodes", parts, incrementRecovery);
            }
        }

        /// <summary>
        /// Records a build in the part tracker
        /// </summary>
        /// <param name="parts">The vessel as a list of Parts.</param>
        public static void RecordBuild(IEnumerable<Part> parts)
        {
            if (Available)
            {
                invokeMethod("RecordBuild_Parts", parts);
            }
        }

        /// <summary>
        /// Records a build in the part tracker
        /// </summary>
        /// <param name="parts">The vessel as a list of ConfigNodes.</param>
        public static void RecordBuild(IEnumerable<ConfigNode> parts)
        {
            if (Available)
            {
                invokeMethod("RecordBuild_Nodes", parts);
            }
        }

        /// <summary>
        /// Takes a List of Parts and returns the Parts that are present in the inventory. 
        /// </summary>
        /// <param name="sourceParts">Source list of parts</param>
        /// <param name="strictness">How strict of a comparison to use. Defaults to MODULES</param>
        /// <returns>List of Parts that are in the inventory</returns>
        public static IList<Part> GetPartsInInventory(IEnumerable<Part> sourceParts, ComparisonStrength strictness = ComparisonStrength.MODULES)
        {
            if (!Available)
            {
                return null;
            }
            return (IList<Part>)invokeMethod("GetPartsInInventory_Parts", sourceParts, strictness.ToString());
            //Why do a ToString on an enum instead of casting to int? Because if the internal enum changes then the intended strictness is kept.
        }

        /// <summary>
        /// Takes a List of part ConfigNodes and returns the ConfigNodes that are present in the inventory. 
        /// </summary>
        /// <param name="sourceParts">Source list of parts</param>
        /// <param name="strictness">How strict of a comparison to use. Defaults to MODULES</param>
        /// <returns>List of part ConfigNodes that are in the inventory</returns>
        public static IList<ConfigNode> GetPartsInInventory(IEnumerable<ConfigNode> sourceParts, ComparisonStrength strictness = ComparisonStrength.MODULES)
        {
            if (!Available)
            {
                return null;
            }
            return (IList<ConfigNode>)invokeMethod("GetPartsInInventory_ConfigNodes", sourceParts, strictness.ToString());
            //Why do a ToString on an enum instead of casting to int? Because if the internal enum changes then the intended strictness is kept.
        }

        /// <summary>
        /// Adds a part to the Inventory
        /// </summary>
        /// <param name="part">The part to add</param>
        /// <param name="incrementRecovery">If true, increments the counter for how many times the part was recovered</param>
        /// <returns>True if added, false otherwise</returns>
        public static bool AddPartToInventory(Part part, bool incrementRecovery)
        {
            if (!Available)
            {
                return false;
            }
            return (bool)invokeMethod("AddPartToInventory_Part", part, incrementRecovery);
        }

        /// <summary>
        /// Adds a part to the Inventory
        /// </summary>
        /// <param name="part">The part to add</param>
        /// <param name="incrementRecovery">If true, increments the counter for how many times the part was recovered</param>
        /// <returns>True if added, false otherwise</returns>
        public static bool AddPartToInventory(ConfigNode part, bool incrementRecovery)
        {
            if (!Available)
            {
                return false;
            }
            return (bool)invokeMethod("AddPartToInventory_Node", part, incrementRecovery);
        }

        /// <summary>
        /// Removes a part from the Inventory using the given strictness for finding the part
        /// </summary>
        /// <param name="part">The part to remove</param>
        /// <param name="strictness">The strictenss to use when searching for the part. Defaults to MODULES</param>
        /// <returns>True if removed, false otherwise.</returns>
        public static bool RemovePartFromInventory(Part part, ComparisonStrength strictness = ComparisonStrength.MODULES)
        {
            if (!Available)
            {
                return false;
            }
            return (bool)invokeMethod("RemovePartFromInventory_Part", part, strictness.ToString());
        }

        /// <summary>
        /// Removes a part from the Inventory using the given strictness for finding the part
        /// </summary>
        /// <param name="part">The part to remove</param>
        /// <param name="strictness">The strictenss to use when searching for the part. Defaults to MODULES</param>
        /// <returns>True if removed, false otherwise.</returns>
        public static bool RemovePartFromInventory(ConfigNode part, ComparisonStrength strictness = ComparisonStrength.MODULES)
        {
            if (!Available)
            {
                return false;
            }
            return (bool)invokeMethod("RemovePartFromInventory_Node", part, strictness.ToString());
        }

        /// <summary>
        /// Finds a part in the inventory for the given part
        /// </summary>
        /// <param name="part">The part to search for</param>
        /// <param name="strictness">The strictness to use when searching for the part. Defaults to MODULES.</param>
        /// <returns>A ConfigNode representing the InventoryPart, or null if none found.</returns>
        public static ConfigNode FindInventoryPart(Part part, ComparisonStrength strictness = ComparisonStrength.MODULES)
        {
            if (!Available)
            {
                return null;
            }
            return invokeMethod("FindInventoryPart_Part", part, strictness.ToString()) as ConfigNode;
        }

        /// <summary>
        /// Finds a part in the inventory for the given part
        /// </summary>
        /// <param name="part">The part to search for</param>
        /// <param name="strictness">The strictness to use when searching for the part. Defaults to MODULES.</param>
        /// <returns>A ConfigNode representing the InventoryPart, or null if none found.</returns>
        public static ConfigNode FindInventoryPart(ConfigNode part, ComparisonStrength strictness = ComparisonStrength.MODULES)
        {
            if (!Available)
            {
                return null;
            }
            return invokeMethod("FindInventoryPart_Node", part, strictness.ToString()) as ConfigNode;
        }

        /// <summary>
        /// Checks if the part is pulled from the inventory or is new
        /// </summary>
        /// <param name="part">The part to check</param>
        /// <returns>True if from inventory, false if new</returns>
        public static bool PartIsFromInventory(Part part)
        {
            if (!Available)
            {
                return false;
            }
            return (bool)invokeMethod("PartIsFromInventory_Part", part);
        }

        /// <summary>
        /// Checks if the part is pulled from the inventory or is new
        /// </summary>
        /// <param name="part">The part to check</param>
        /// <returns>True if from inventory, false if new</returns>
        public static bool PartIsFromInventory(ConfigNode part)
        {
            if (!Available)
            {
                return false;
            }
            return (bool)invokeMethod("PartIsFromInventory_Node", part);
        }

        /// <summary>
        /// Gets the number of builds for a part
        /// </summary>
        /// <param name="part">The part to check</param>
        /// <returns>Number of builds for the part</returns>
        public static int GetBuildCount(Part part)
        {
            if (!Available)
            {
                return 0;
            }
            return (int)invokeMethod("GetBuildCount_Part", part);
        }

        /// <summary>
        /// Gets the number of builds for a part
        /// </summary>
        /// <param name="partNode">The ConfigNode of the part to check</param>
        /// <returns>Number of builds for the part</returns>
        public static int GetBuildCount(ConfigNode part)
        {
            if (!Available)
            {
                return 0;
            }
            return (int)invokeMethod("GetBuildCount_Node", part);
        }

        /// <summary>
        /// Gets the number of total uses of a part
        /// </summary>
        /// <param name="part">The part to check</param>
        /// <returns>Number of uses of the part</returns>
        public static int GetUseCount(Part part)
        {
            if (!Available)
            {
                return 0;
            }
            return (int)invokeMethod("GetUseCount_Part", part);
        }

        /// <summary>
        /// Gets the number of total uses of a part
        /// </summary>
        /// <param name="partNode">The ConfigNode of the part to check</param>
        /// <returns>Number of uses of the part</returns>
        public static int GetUseCount(ConfigNode part)
        {
            if (!Available)
            {
                return 0;
            }
            return (int)invokeMethod("GetUseCount_Node", part);
        }

        #region Private Methods
        /// <summary>
        /// The static instance of the APIManager within ScrapYard
        /// </summary>
        private static object Instance
        {
            get
            {
                if (Available && _instance == null)
                {
                    _instance = SYType.GetProperty("Instance").GetValue(null, null);
                }
                return _instance;
            }
        }

        /// <summary>
        /// Invokes a method on the ScrapYard API
        /// </summary>
        /// <param name="methodName">The name of the method</param>
        /// <param name="parameters">Parameters to pass to the method</param>
        /// <returns>The response</returns>
        private static object invokeMethod(string methodName, params object[] parameters)
        {
            MethodInfo method = SYType.GetMethod(methodName);
            return method?.Invoke(Instance, parameters);
        }
        #endregion Private Methods
    }
}
