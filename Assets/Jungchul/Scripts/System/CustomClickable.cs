using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;
using System;
using Unity.VisualScripting;

[RequireComponent(typeof(SpriteRenderer))]
public class CustomClickable : MonoBehaviour
{
    // Ŭ�� �̺�Ʈ �ܺ� ����
    public Action onClick;

    // ��������Ʈ �����
    public Sprite normalSprite;
    public Sprite hoverSprite;

    private SpriteRenderer spriteRenderer;
    
    private bool isMouseOver = false;

    public Func<bool> interactionCondition = () => true;



    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            Debug.LogWarning("SpriteRenderer�� �����ϴ�.");
        spriteRenderer.sprite = normalSprite;
    }

    private void OnMouseEnter()
    {
        isMouseOver = true;
        print("���콺 �ѿ���!");
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
        if (!isMouseOver) return;

        if (interactionCondition != null && !interactionCondition.Invoke())
        {
            Debug.Log($"{name}: Ŭ�� ������ �������� ����.");
            return;
        }

        onClick?.Invoke();
    }


    public void SetClickAction(Action action)
    {
        onClick = action;
    }

    /// <summary>
    /// GuildManager�� bool ������ ���÷������� üũ (�����ϰ� ���� �̸� ���� ����)
    /// </summary>
    public void SetInteractionCondition(Func<bool> condition)
    {
        interactionCondition = condition ?? (() => true);
    }
}