using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopBarRelicController : MonoBehaviour
{

    [Header("Object Assignments")]
    [SerializeField] private Transform _relicContainerParentTransform;
    private Dictionary<RelicType, GameObject> _relicReferences = new Dictionary<RelicType, GameObject>();

    public void RenderRelics()
    {
        // Return all current relics to the pool.
        foreach (GameObject obj in _relicReferences.Values)
        {
            ObjectPooler.Instance.ReturnObjectToPool(PoolableType.RELIC, obj);
        }
        _relicReferences.Clear();
        // Spawn all relic objects at top.
        foreach (Relic r in GameManager.GetRelics())
        {
            GameObject relicObj = ObjectPooler.Instance.GetObjectFromPool(PoolableType.RELIC);
            relicObj.transform.SetParent(_relicContainerParentTransform, false);
            relicObj.GetComponent<RelicHandler>().Initialize(r, true);
            _relicReferences[r.type] = relicObj;
        }
    }

    public void FlashRelicObject(RelicType relicType)
    {
        if (!_relicReferences.ContainsKey(relicType))
        {
            Debug.Log("ERROR IN TOPBARRELICCONTROLLER.CS > GETRELICOBJECT! COULD NOT FIND RELIC (" + relicType + ")!");
            return;
        }
        _relicReferences[relicType].GetComponent<RelicHandler>().FlashRelic();
        return;
    }

}
