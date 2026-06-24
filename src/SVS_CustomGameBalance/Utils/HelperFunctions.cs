using System;
using System.Collections.Generic;
using System.Linq;
using Character;
using Manager;
using SaveData;
using SV.H;

namespace SVS_CustomGameBalance
{
    internal static class HelperFunctions
    {
        public static int TryGetActorId(this Actor currentAdvChara)
        {
            if (currentAdvChara == null) throw new ArgumentNullException(nameof(currentAdvChara));

            var found = Game.Charas.AsManagedEnumerable().FirstOrDefault(x => currentAdvChara.Equals(x.Value));
            return found.Value != null ? found.Key : -1;
        }

        /// <summary>
        /// Get the main character instance of the actor (the one that is visible on the main map and saved to the save file).
        /// </summary>
        public static KeyValuePair<int, Actor> FindMainActorInstance(this HActor x) => x?.Actor.FindMainActorInstance() ?? default;

        /// <summary>
        /// Get the main character instance of the actor (the one that is visible on the main map and saved to the save file).
        /// </summary>
        public static KeyValuePair<int, Actor> FindMainActorInstance(this Actor x) => x?.charFile.About.FindMainActorInstance() ?? default;

        /// <summary>
        /// Get the main character instance of the actor (the one that is visible on the main map and saved to the save file).
        /// TODO: Find a better way to get the originals
        /// </summary>
        public static KeyValuePair<int, Actor> FindMainActorInstance(this HumanDataAbout x) => x == null ? default : Game.Charas.AsManagedEnumerable().FirstOrDefault(y => x.dataID == y.Value.charFile.About.dataID);

        public static KeyValuePair<int, Actor> FindMainActorInstanceMemory(this MemoryParameter x) => x == null ? default : Game.Charas.AsManagedEnumerable().FirstOrDefault(y => x.Pointer == y.Value.charasGameParam.memory.Pointer);

        public static IEnumerable<T> AsManagedEnumerable<T>(this Il2CppSystem.Collections.Generic.List<T> collection)
        {
            foreach (var val in collection)
                yield return val;
        }
        //public static IEnumerable<T> AsManagedEnumerable<T>(this Il2CppSystem.Collections.Generic.HashSet<T> collection)
        //{
        //    foreach (var val in collection)
        //        yield return val;
        //}
        public static IEnumerable<KeyValuePair<T1, T2>> AsManagedEnumerable<T1, T2>(this Il2CppSystem.Collections.Generic.Dictionary<T1, T2> collection)
        {
            foreach (var val in collection)
                yield return new KeyValuePair<T1, T2>(val.Key, val.Value);
        }
    }
}
