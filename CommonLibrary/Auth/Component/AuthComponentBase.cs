using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.Auth
{
    public class AuthComponentBase :ComponentBase
    {
        [Inject]
        public AuthService authService { get; set; }
        private bool uiProcessing = false;
        public bool UIProcessing => uiProcessing;

        public void UIBlock()
        {
            uiProcessing = true;
        }

        public void UIRelease()
        {
            uiProcessing = false;
        }

        protected override async Task OnInitializedAsync()
        {
            authService.AuthStatusChangedAct += UIUpdate;
            await authService.RetriveUserInfo();
            await base.OnInitializedAsync();
        }

        private void UIUpdate()
        {
            InvokeAsync(StateHasChanged);
        }
    }
}
