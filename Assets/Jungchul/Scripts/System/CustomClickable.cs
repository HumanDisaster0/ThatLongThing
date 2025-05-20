using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;
using System;
using Unity.VisualScripting;

[RequireComponent(typeof(SpriteRenderer))]
public class CustomClickable : MonoBehaviour
{
    // 클릭 이벤트 외부 지정
    public Action onClick;

    // 스프라이트 변경용
    public Sprite normalSprite;
    public Sprite hoverSprite;

    private SpriteRenderer spriteRenderer;
    
    private bool isMouseOver = false;

    public Func<bool> interactionCondition = () => true;

    public bool isInteractable = true;



    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            Debug.LogWarning("SpriteRenderer가 없습니다.");
        spriteRenderer.sprite = normalSprite;
    }

    private void OnMouseEnter()
    {
        isMouseOver = true;
        //print("마우스 롤오버!");
        if (hoverSprite != null)
        {           
            spriteRenderer.sprite = hoverSprite;
        }
    }

    private void OnMouseExit()
    {
        isMouseOver = false;
        if (normalSprite != null)
            spriteRenderer.sprite = normalSprite;
    }

    private void OnMouseUpAsButton()
    {
        if (!isMouseOver || !isInteractable) return;


        //Debug.Log("[CustomClickable] 클릭됨");

        if (interactionCondition != null && !interactionCondition.Invoke())
        {
            Debug.Log($"{name}: 클릭 조건이 충족되지 않음.");
            return;
        }

        onClick?.Invoke();
    }


    public void SetClickAction(Action action)
    {
        onClick = action;
    }

    /// <summary>
    /// GuildManager의 bool 조건을 리플렉션으로 체크 (유연하게 변수 이름 지정 가능)
    /// </summary>
    public void SetInteractionCondition(Func<bool> condition)
    {
        interactionCondition = condition ?? (() => true);
    }

    public void SetSprites(Sprite normal, Sprite hover)
    {
        normalSprite = normal;
        hoverSprite = hover;
        if (spriteRenderer != null)
            spriteRenderer.sprite = normal;
    }

    public void AdjustColliderToSprite()
    {
        var sr = GetComponent<SpriteRenderer>();
        var col = GetComponent<BoxCollider2D>();

        if (sr != null && sr.sprite != null && col != null)
        {
            col.size = sr.sprite.bounds.size;
            col.offset = sr.sprite.bounds.center;
        }
    }
}