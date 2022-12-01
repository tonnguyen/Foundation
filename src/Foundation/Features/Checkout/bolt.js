async function initBoltCheckout(key, isAuthenticated, username, isProduction) {
    try {
        const apiUrl = isProduction ? "api.bolt.com" : "api-sandbox.bolt.com";
        const boltEmbedded = Bolt(key);
        if (isAuthenticated) {
            const loginStatusComponent = boltEmbedded.create("login_status");
            await loginStatusComponent.mount("#login-status");
            var response = await fetch("https://" + apiUrl + "/v1/account/exists?email=" + encodeURIComponent(username))
            if (response) {
                var responseAsJson = await response.json();
                if (responseAsJson) {
                    if (responseAsJson.has_bolt_account) {
                        const authorizationComponent = boltEmbedded.create("authorization_component", { style: { position: "center" } });
                        await authorizationComponent.mount("#boltAccount");
                        let authResponse = await authorizationComponent.authorize({ "email": username });
                        if (authResponse) {
                            let account = await fetch(window.location + 'GetBoltCards?code=' + authResponse.authorizationCode + '&scope=' + authResponse.scope);
                            let data = await account.json();
                            if (data) {
                                if (data?.payment_methods?.length > 0) {
                                    loadPaymentMethods(data);
                                }
                                else {
                                    document.getElementById("boltCards").style.display = "none";
                                    loadPaymentFields(boltEmbedded, data);
                                }
                            }
                            
                        }
                        return;
                    }
                    else {
                        loadPaymentFields(boltEmbedded);
                    }
                }
            }
        }
        else {
            loadPaymentFields(boltEmbedded);
        }
    }
    catch (error) {
        console.log("these are errors", error)
    }
}

function loadPaymentFields(boltEmbedded, account) {
    let cards = document.getElementById("boltCards");
    if (cards) {
        cards.style.display = "none";
    }

    var paymentComponent = boltEmbedded.create("payment_component");
    paymentComponent.mount("#boltPayment");

    if (!account) {
        const accountCheckboxComponent = boltEmbedded.create("account_checkbox");
        accountCheckboxComponent.mount("#boltCheckbox");
    }
    
    const btn = document.querySelector('.jsAddPayment');
    btn.addEventListener("click", async (e) => {
        e.preventDefault();
        let url = e.target.getAttribute('url');
        let checked = document.querySelector('input[name = "PaymentMethodType"]:checked');;
        let methodId = checked.getAttribute('methodId');
        let keyword = checked.getAttribute('keyword');
        
        if (keyword !== 'Bolt') {
            return;
        }

        const tokenize = await paymentComponent.tokenize();
        if (tokenize) {
            
            let data = new FormData();
            data.set('PaymentMethodId', methodId);
            data.set('SystemKeyword', keyword);
            data.set('CreateAccount', document.getElementById('bolt-acct-check').checked ? "true" : "false");
            data.set('Token', JSON.stringify(tokenize));
            if (account?.token) {
                data.set('AccessToken', account.token); ;
            }

            axios.post(url, data)
                .then(function (result) {
                    if (result.status != 200) {
                        notification.error(result);
                    } else {
                        $('#paymentBlock').html(result.data);
                        feather.replace();
                        inst.initPayment();
                    }
                })
                .catch(function (error) {
                    if (error?.response && error.response.status == 400) {
                        $("#giftcard-alert").html(error.response.statusText);
                        $("#giftcard-alert").removeClass("alert-info");
                        $("#giftcard-alert").addClass("alert-danger");
                    } else {
                        notification.error(error);
                    }
                });
        }
    });
}

function loadPaymentMethods(account) {
    document.getElementById("boltCards").style.display = "block";
    document.getElementById("boltPayment").style.display = "none";
    document.getElementById("boltCheckbox").style.display = "none";
    
    var select = document.getElementById("selectCard");
    account.payment_methods.forEach((method) => {
        var option = document.createElement("option");
        option.text = method.network + ' - ' + method.last4;
        option.value = method.id;
        select.options.add(option);

    });
    const btn = document.querySelector('.jsAddPayment');
    btn.addEventListener("click", async (e) => {
        e.preventDefault();
        let url = e.target.getAttribute('url');
        let checked = document.querySelector('input[name = "PaymentMethodType"]:checked');;
        let methodId = checked.getAttribute('methodId');
        let keyword = checked.getAttribute('keyword');

        if (keyword !== 'Bolt') {
            return;
        }

        if (select && select.value !== '0') {
            let data = new FormData();
            data.set('PaymentMethodId', methodId);
            data.set('SystemKeyword', keyword);
            data.set('CardId', select.value);
            if (account.token) {
                data.set('AccessToken', account.token);;
            }

            axios.post(url, data)
                .then(function (result) {
                    if (result.status != 200) {
                        notification.error(result);
                    } else {
                        $('#paymentBlock').html(result.data);
                        feather.replace();
                        inst.initPayment();
                    }
                })
                .catch(function (error) {
                    if (error?.response && error.response.status == 400) {
                        $("#giftcard-alert").html(error.response.statusText);
                        $("#giftcard-alert").removeClass("alert-info");
                        $("#giftcard-alert").addClass("alert-danger");
                    } else {
                        notification.error(error);
                    }
                });
        }
    });
}