using System;
using System.Collections.Generic;

namespace banging_code.common
{
    public class ID
    {
        public readonly string Prefix;

        public readonly bool IsLazy;

        public bool IsGenerated { get; private set; }

        private string id;

        private static readonly HashSet<string> currentIDs = new();

        public ID(string prefix = "", bool isLazy = false)
        {
            Prefix = prefix;
            IsLazy = isLazy;
            
            if(!IsLazy)
            {
                GenerateUnicID();
            }
        }

        ~ID() => currentIDs.Remove(id);

        public override int GetHashCode()
        {
            return HashCode.Combine(Prefix, IsLazy, id, IsGenerated);
        }

        public override bool Equals(object obj)
        {
            if (obj is ID otherID) { return otherID.Get() == Get(); }
            return false;
        }

        public static bool operator ==(ID id1, ID id2)
        {
            if (ReferenceEquals(id1, id2)) return true;
            if (ReferenceEquals(id1, null)) return false;
            if (ReferenceEquals(id2, null)) return false; 
            
            return id1.Get() == id2.Get();
        }
        public static bool operator != (ID id1, ID id2) { return !(id1 == id2); }

        public string Get()
        {
            if(!IsGenerated && IsLazy)
            {
                GenerateUnicID();
            }

            return id;
        }

        private void GenerateUnicID()
        {
            string tempPrefix = "";
            
            if (Prefix != "")
            {
                tempPrefix = Prefix + "_";
            }

            do
            {
                id = tempPrefix + GetRandomID();
            } while (currentIDs.Contains(id));

            currentIDs.Add(id);
            IsGenerated = true;
        }
        
        public static string GetRandomID()
        {
            string ID = "";
            
            ID += UnityEngine.Random.Range(0, 10);
            ID += UnityEngine.Random.Range(0, 10);
            ID += UnityEngine.Random.Range(0, 10);
            ID += UnityEngine.Random.Range(0, 10);
            
            ID += UnityEngine.Random.Range(0, 10);
            ID += UnityEngine.Random.Range(0, 10);
            ID += UnityEngine.Random.Range(0, 10);
            ID += UnityEngine.Random.Range(0, 10);

            return ID;
        }
    }
}