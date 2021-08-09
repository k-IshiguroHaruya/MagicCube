
using UnityEngine;
using UnityEngine.UI;
using System;

namespace MagicCube
{
    public class ConfirmDialogController : MonoBehaviour
    {
        [SerializeField] private Text title;
        [SerializeField] private Button yesButton;
        [SerializeField] private Text yesButtonText;
        [SerializeField] private Button noButton;
        [SerializeField] private Text noButtonText;

        public void SetUI( string title, Action yesButtonAction, string yesButtonText, Action noButtonAction, string noButtonText )
        {
            this.title.text = title;
            yesButton.onClick.AddListener( () => yesButtonAction() );
            this.yesButtonText.text = yesButtonText;
            noButton.onClick.AddListener( () => noButtonAction() );
            this.noButtonText.text = noButtonText;
        }

    }
}
