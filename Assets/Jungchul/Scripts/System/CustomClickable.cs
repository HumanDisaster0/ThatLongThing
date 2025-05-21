using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.UI;
using System;
using Unity.VisualScripting;

//[RequireComponent(typeof(SpriteRenderer))]
public class CustomClickable : MonoBehaviour
{
    // 클릭 이벤트 외부 지정
    public Action onClick;

    // 스프라이트 변경용
    public Sprite normalSprite;
    public Sprite hoverSprite;

    // 사운드태그
    public string soundTag = "Default";

    private SpriteRenderer spriteRenderer;
    private Image imageRenderer;
    
    private bool isMouseOver = false;

    public Func<bool> interactionCondition = () => true;

    public bool isInteractable = true;
    bool prevInteractable = true;



    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        imageRenderer = GetComponent<Image>();
        if (!spriteRenderer && !imageRenderer)
            Debug.LogWarning("SpriteRenderer나 ImageRenderer가 없습니다.");
            
        if(spriteRenderer)
            spriteRenderer.sprite = normalSprite;
        if(imageRenderer)
            imageRenderer.sprite = normalSprite;

        soundTag.NullIfEmpty();
    }

    private void OnMouseEnter()
    {
        isMouseOver = true;
        //print("마우스 롤오버!");

        if(soundTag != null)
        {
            UiSoundManager.instance?.PlaySound(soundTag, UISFX.Hover);
        }

        if(spriteRenderer)
        {
            if (hoverSprite != null && isInteractable)
            {
                spriteRenderer.sprite = hoverSprite;
            }
        }

        if (imageRenderer)
        {
            if (hoverSprite != null && isInteractable)
            {
                imageRenderer.sprite = hoverSprite;
            }
        }
    }

    private void OnMouseExit()
    {
        if (soundTag != null)
        {
            UiSoundManager.instance?.PlaySound(soundTag, UISFX.Exit);
        }

        isMouseOver = false;
        if (spriteRenderer)
        {
            if (normalSprite != null)
                spriteRenderer.sprite = normalSprite;
        }

        if (imageRenderer)
        {
            if (normalSprite != null)
                imageRenderer.sprite = normalSprite;
        }
    }

    private void OnMouseUpAsButton()
    {
        if (!isMouseOver || !isInteractable) return;


        //Debug.Log("[CustomClickable] 클릭됨");

        if (soundTag != null)
        {
            UiSoundManager.instance?.PlaySound(soundTag, UISFX.Click);
        }

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
        var ir = GetComponent<Image>();
        var col = GetComponent<BoxCollider2D>();

        if (sr != null && sr.sprite != null && col != null)
        {
            col.size = sr.sprite.bounds.size;
            col.offset = sr.sprite.bounds.center;
        }

        if (sr != null && ir.sprite != null && col != null)
        {
            col.size = ir.sprite.bounds.size;
            col.offset = ir.sprite.bounds.center;
        }
    }

    private void Update()
    {
        if(prevInteractable != isInteractable)
        {
            prevInteractable = isInteractable;
            UpdateSprite();
        }
    }

    private void UpdateSprite()
    {
        if(spriteRenderer)
        {
            if (!isInteractable && normalSprite)
            {
                GetComponent<SpriteRenderer>().color = Color.gray;
            }
            else if (normalSprite)
            {
                GetComponent<SpriteRenderer>().color = Color.white;
            }
        }
        
        if(imageRenderer)
        {
            if (!isInteractable && normalSprite)
            {
                GetComponent<Image>().color = Color.gray;
            }
            else if (normalSprite)
            {
                GetComponent<Image>().color = Color.white;
            }
        }
    }
}