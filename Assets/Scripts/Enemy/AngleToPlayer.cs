using System;
using UnityEngine;

public class AngleToPlayer : MonoBehaviour
{
    private Transform localPlayer;
    private Vector3 targetPos;
    private Vector3 targetDir;
    
    private SpriteRenderer spriteRenderer;
    
    private float angle;
    public int lastIndex;
    
    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        FindLocalPlayer();
    }
    
    // Update is called once per frame
    void Update()
    {
        if (localPlayer == null)
        {
            FindLocalPlayer();
            if (localPlayer == null) return;
        }
        
        // Get Target Position and Direction
        targetPos = new Vector3(localPlayer.position.x, transform.position.y, localPlayer.position.z);
        targetDir = targetPos - transform.position;
        
        // Get Angle
        angle = Vector3.SignedAngle(targetDir, transform.forward, Vector3.up);
        
        // Flip Sprite if needed
        if (spriteRenderer != null)
        {
            Vector3 tempScale = Vector3.one;
            if (angle > 0)
            {
                tempScale.x *= -1f;
            }
            spriteRenderer.transform.localScale = tempScale;
        }
        
        lastIndex = MathUtils.Get8WayAngleIndex(angle);
    }
    
    private void FindLocalPlayer()
    {
        localPlayer = NetworkUtils.GetLocalPlayer()?.transform;
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, transform.forward);
    }
}