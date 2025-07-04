using System.Collections;
using UnityEngine;

public abstract class EventBase : MonoBehaviour
{
     public abstract IEnumerator Execute(ButtonManager manager);
}
