using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BattlePooler : MonoBehaviour
{

    public static BattlePooler Instance;
    [Header("Prefab Assignments")]
    public GameObject cardPrefab;
    public GameObject statusPrefab;
    public GameObject oneShotPrefab;
    public GameObject projectilePrefab;
    public GameObject projectileParticlePrefab;
    public Transform pooledObjectParentTransform;
    private Stack<GameObject> _inactiveCardObjects = new Stack<GameObject>();
    private Stack<GameObject> _inactiveStatusObjects = new Stack<GameObject>();
    private Stack<GameObject> _inactiveOneShotObjects = new Stack<GameObject>();
    private Stack<GameObject> _inactiveProjectileObjects = new Stack<GameObject>();
    private Stack<GameObject> _inactiveProjectileParticleObjects = new Stack<GameObject>();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    /*  
        BATTLE CARD OBJECTS
    */

    private void AddCardObjectToPool()
    {
        GameObject cardObject = Instantiate(cardPrefab);
        // Disable the Canvas component and add it to our deactivated list.
        cardObject.SetActive(false);
        _inactiveCardObjects.Push(cardObject);
    }

    public GameObject GetCardObjectFromPool(Transform parentTransform)
    {
        if (_inactiveCardObjects.Count == 0)
        {
            AddCardObjectToPool();
        }
        // Return an already created card object.
        GameObject cardObject = _inactiveCardObjects.Pop();
        cardObject.SetActive(true);
        cardObject.transform.SetParent(parentTransform, false);
        cardObject.transform.localScale = new Vector2(0.5f, 0.5f);
        cardObject.transform.position = parentTransform.position;
        cardObject.transform.rotation = Quaternion.identity;
        cardObject.tag = "CardUI";
        return cardObject;
    }

    public void ReturnCardObjectToPool(GameObject objectToDeactivate)
    {
        objectToDeactivate.SetActive(false);
        objectToDeactivate.transform.SetParent(pooledObjectParentTransform);
        _inactiveCardObjects.Push(objectToDeactivate);
    }

    /*  
        BATTLE STATUS OBJECTS
    */

    private void AddStatusObjectToPool()
    {
        GameObject statusObject = Instantiate(statusPrefab);
        // Disable GameObject and add it to our deactivated list.
        statusObject.SetActive(false);
        _inactiveStatusObjects.Push(statusObject);
    }

    public GameObject GetStatusObjectFromPool()
    {
        if (_inactiveStatusObjects.Count == 0)
        {
            AddStatusObjectToPool();
        }
        // Return an already created status object.
        GameObject statusObject = _inactiveStatusObjects.Pop();
        statusObject.SetActive(true);
        return statusObject;
    }

    public void ReturnStatusObjectToPool(GameObject objectToDeactivate)
    {
        objectToDeactivate.SetActive(false);
        objectToDeactivate.transform.SetParent(pooledObjectParentTransform);
        _inactiveStatusObjects.Push(objectToDeactivate);
    }

    /*  
        BATTLE EXPLOSIONS OBJECTS
    */

    private void AddOneShotAnimationObjectToPool()
    {
        GameObject oneShotObject = Instantiate(oneShotPrefab);
        // Disable GameObject and add it to our deactivated list.
        oneShotObject.SetActive(false);
        _inactiveOneShotObjects.Push(oneShotObject);
    }

    public void StartOneShotAnimationFromPool(Vector3 positionToSpawnAt, string animName)
    {
        if (_inactiveOneShotObjects.Count == 0)
        {
            AddOneShotAnimationObjectToPool();
        }
        // Return an already created one shot object.
        GameObject oneShotObject = _inactiveOneShotObjects.Pop();
        oneShotObject.SetActive(true);
        oneShotObject.transform.position = positionToSpawnAt;
        StartCoroutine(AnimateOneShotObjectCoroutine(oneShotObject, animName));
    }

    private IEnumerator AnimateOneShotObjectCoroutine(GameObject obj, string animName)
    {
        // Animate the one shot object.
        Animator objAnimator = obj.GetComponent<Animator>();
        objAnimator.Play(animName);
        // Wait until the animator is done playing the animation.
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => { return objAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f; });
        // Return the one shot object to the pool.
        obj.SetActive(false);
        obj.transform.SetParent(pooledObjectParentTransform);
        _inactiveOneShotObjects.Push(obj);
    }

    /* 
        BATTLE PROJECTILE OBJECTS
    */

    private void AddProjectileObjectToPool()
    {
        GameObject projectileObject = Instantiate(projectilePrefab);
        // Disable GameObject and add it to our deactivated list.
        projectileObject.SetActive(false);
        _inactiveProjectileObjects.Push(projectileObject);
    }

    public void StartProjectileAnimationFromPool(Vector3 positionToSpawnAt, Vector3 targetPosition, Sprite projectileSprite, float timeToReachTarget, bool projectileGoInFront = false)
    {
        if (_inactiveProjectileObjects.Count == 0)
        {
            AddProjectileObjectToPool();
        }
        // Return an already created one shot object.
        GameObject projectileObject = _inactiveProjectileObjects.Pop();
        projectileObject.SetActive(true);
        projectileObject.transform.position = positionToSpawnAt;
        projectileObject.GetComponent<SpriteRenderer>().sprite = projectileSprite;
        projectileObject.GetComponent<SpriteRenderer>().sortingOrder = (projectileGoInFront) ? 99 : -1;
        StartCoroutine(AnimateProjectileCoroutine(projectileObject, targetPosition, timeToReachTarget));
    }

    private IEnumerator AnimateProjectileCoroutine(GameObject obj, Vector3 targetPosition, float timeToReachTarget)
    {
        // Animate the projectile object.
        float currTime = 0;
        Vector3 initialPosition = obj.transform.position;
        while (currTime < timeToReachTarget)
        {
            currTime += Time.deltaTime;
            obj.transform.position = Vector3.Lerp(initialPosition, targetPosition, currTime / timeToReachTarget);
            obj.transform.Rotate(0, 0, 8);
            yield return null;
        }
        // Return the one shot object to the pool.
        obj.SetActive(false);
        obj.transform.SetParent(pooledObjectParentTransform);
        _inactiveProjectileObjects.Push(obj);
    }

    /* 
        BATTLE PROJECTILE PARTICLE OBJECTS
    */

    private void AddProjectileParticleObjectToPool()
    {
        GameObject projectileParticleObject = Instantiate(projectileParticlePrefab);
        // Disable GameObject and add it to our deactivated list.
        projectileParticleObject.SetActive(false);
        _inactiveProjectileParticleObjects.Push(projectileParticleObject);
    }

    public void StartParticleAnimationFromPool(ParticleInfo particleInfo, Vector3 positionToSpawnAt, int particleBurstDirectionX)
    {
        if (_inactiveProjectileParticleObjects.Count == 0)
        {
            AddProjectileParticleObjectToPool();
        }
        // Return an already created one shot object.
        GameObject projectileParticleObject = _inactiveProjectileParticleObjects.Pop();
        projectileParticleObject.SetActive(true);
        ProjectileParticleHandler objPPH = projectileParticleObject.GetComponent<ProjectileParticleHandler>();
        ParticleInfo pInfo = particleInfo;
        objPPH.SummonParticle(pInfo, positionToSpawnAt, particleBurstDirectionX, (obj) =>
        {
            // After the particles have spawned, disable this again.
            obj.SetActive(false);
            obj.transform.SetParent(pooledObjectParentTransform);
            _inactiveProjectileParticleObjects.Push(obj);
        });
    }

}
