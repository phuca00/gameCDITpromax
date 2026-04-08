using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectPlayerManager : MonoBehaviour
{
    public Transform selectFrame;
    public Transform[] playerSlots;

    private int currentIndex = 0;
    private bool isConfirmed = false;

    private SpriteRenderer frameRenderer;

    void Start()
    {
        frameRenderer = selectFrame.GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        HandleKeyboard();
        HandleMouse();
    }

    void HandleMouse()
    {
        if (Input.GetMouseButtonDown(0) && !isConfirmed)
        {
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D hit = Physics2D.OverlapPoint(worldPos);

            if (hit != null)
            {
                SelectablePlayer sel = hit.GetComponent<SelectablePlayer>();
                if (sel != null)
                {
                    MoveSelectFrame(sel.index);
                }
            }
        }
    }

    void HandleKeyboard()
    {
        if (!isConfirmed)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
                MoveSelectFrame(Mathf.Min(currentIndex + 1, playerSlots.Length - 1));

            if (Input.GetKeyDown(KeyCode.LeftArrow))
                MoveSelectFrame(Mathf.Max(currentIndex - 1, 0));

            if (Input.GetKeyDown(KeyCode.DownArrow))
                MoveSelectFrame(Mathf.Min(currentIndex + 2, playerSlots.Length - 1));

            if (Input.GetKeyDown(KeyCode.UpArrow))
                MoveSelectFrame(Mathf.Max(currentIndex - 2, 0));
        }

        // ENTER để xác nhận
        if (Input.GetKeyDown(KeyCode.Return) && !isConfirmed)
        {
            ConfirmSelection();
        }
    }

    void MoveSelectFrame(int newIndex)
    {
        currentIndex = newIndex;
        selectFrame.position = playerSlots[currentIndex].position;
    }

    void ConfirmSelection()
    {
        isConfirmed = true;

        PlayerPrefs.SetInt("SelectedPlayerIndex", currentIndex);
        PlayerPrefs.Save();

        Debug.Log("Confirmed Player: " + currentIndex);

        StartCoroutine(ConfirmEffect());
        StartCoroutine(BlinkEffect()); // 🔥 thêm blink
    }

    // ✨ hiệu ứng phóng to
    IEnumerator ConfirmEffect()
    {
        Vector3 original = selectFrame.localScale;
        Vector3 target = original * 1.3f;

        float t = 0;
        while (t < 0.15f)
        {
            t += Time.deltaTime;
            selectFrame.localScale = Vector3.Lerp(original, target, t / 0.15f);
            yield return null;
        }

        t = 0;
        while (t < 0.15f)
        {
            t += Time.deltaTime;
            selectFrame.localScale = Vector3.Lerp(target, original, t / 0.15f);
            yield return null;
        }
    }

    // ✨ hiệu ứng nhấp nháy
    IEnumerator BlinkEffect()
    {
        while (isConfirmed)
        {
            frameRenderer.enabled = false;
            yield return new WaitForSeconds(0.2f);

            frameRenderer.enabled = true;
            yield return new WaitForSeconds(0.2f);
        }
    }
}