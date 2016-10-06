﻿// <copyright file="SignInPageLoginBehaviour.cs" company="Automate The Planet Ltd.">
// Copyright 2016 Automate The Planet Ltd.
// Licensed under the Apache License, Version 2.0 (the "License");
// You may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
// <author>Anton Angelov</author>
// <site>http://automatetheplanet.com/</site>

using Microsoft.Practices.Unity;
using PerfectSystemTestsDesign.Data;
using PerfectSystemTestsDesign.Pages.ShippingAddressPage;
using PerfectSystemTestsDesign.Pages.SignInPage;
using PerfectSystemTestsDesign.SpecflowBehaviours.Core;
using TechTalk.SpecFlow;

namespace PerfectSystemTestsDesign.SpecflowBehaviours
{
    [Binding]
    public class SignInPageLoginBehaviour : WaitableActionBehaviour
    {
        private readonly SignInPage signInPage;
        private readonly ShippingAddressPage shippingAddressPage;
        private ClientLoginInfo clientLoginInfo;

        public SignInPageLoginBehaviour()
        {
            this.signInPage = PerfectSystemTestsDesign.Base.UnityContainerFactory.GetContainer().Resolve<SignInPage>();
            this.shippingAddressPage = PerfectSystemTestsDesign.Base.UnityContainerFactory.GetContainer().Resolve<ShippingAddressPage>(); 
        }

        [When(@"I login with email = ""([^""]*)"" and pass = ""([^""]*)""")]
        public void LoginWithEmailAndPass(string email, string password)
        {
            this.clientLoginInfo = new ClientLoginInfo
            {
                Email = email,
                Password = password
            };
            base.Execute();
        }

        protected override void PerformPostActWait()
        {
            this.shippingAddressPage.WaitForPageToLoad();
        }

        protected override void PerformAct()
        {
            this.signInPage.Login(this.clientLoginInfo.Email, this.clientLoginInfo.Password);
        }
    }
}