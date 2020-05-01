using System.Collections.Generic;
using UnityEngine;

namespace Osteogenesis
{
    public class Utility
    {
        
        public static void PrintList(List<Object> list)
        {
            foreach (var entry in list)
            {
                Debug.Log(entry);
            }
        }
    }
}