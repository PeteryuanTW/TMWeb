﻿using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMWeb.Services;

namespace CommonLibrary.Auth.Component
{
    public class AuthComponentBase :ComponentBase
    {
        [Inject]
        public AuthService authService { get; set; }
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
