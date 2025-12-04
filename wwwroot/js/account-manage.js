"use strict";

document.addEventListener("DOMContentLoaded", () => {
    const userJson = localStorage.getItem("user");
    if (!userJson) {
        alert("You must be logged in to view this page.");
        window.location.href = "/Account/Login";
        return;
    }

    const user = JSON.parse(userJson);
    const userId = user.id;

    // --- Admin button visibility ---
    const adminLink = document.getElementById("admin-panel-link");
    if (adminLink) {
        const raw = user.isAdmin;

        // Debug once so we can see what's coming from localStorage
        console.log("user.isAdmin from localStorage:", raw, "type:", typeof raw);

        const isAdmin =
            raw === true ||
            raw === 1 ||
            raw === "1" ||
            raw === "True" ||
            raw === "true";

        if (!isAdmin) {
            adminLink.remove();      // hard block for non-admins
        } else {
            adminLink.style.display = "block";
        }
    }

    // --- DOM references ---
    const nameSpan = document.getElementById("user-name");
    const emailSpan = document.getElementById("user-email");
    const balanceSpan = document.getElementById("user-balance");
    const paymentMethodsContainer = document.getElementById("payment-methods-container");

    const refreshBalanceBtn = document.getElementById("refresh-balance-btn");
    const changeEmailBtn = document.getElementById("change-email-btn");

    const paymentMethodsModalEl = document.getElementById("paymentMethodsModal");
    const addPaymentMethodModalEl = document.getElementById("addPaymentMethodModal");
    const changeEmailModalEl = document.getElementById("changeEmailModal");
    const changePasswordModalEl = document.getElementById("changePasswordModal");

    const addFundsModalEl = document.getElementById("addFundsModal");

    const paymentMethodsModal = new bootstrap.Modal(paymentMethodsModalEl);
    const addPaymentMethodModal = new bootstrap.Modal(addPaymentMethodModalEl);
    const changeEmailModal = new bootstrap.Modal(changeEmailModalEl);
    const changePasswordModal = new bootstrap.Modal(changePasswordModalEl);

    let addFundsModal = null;
    if (addFundsModalEl) {
        addFundsModal = new bootstrap.Modal(addFundsModalEl);
    }

    // --- Initial UI from localStorage ---
    nameSpan.textContent = `${user.firstName} ${user.lastName}`;
    emailSpan.textContent = user.email;
    balanceSpan.textContent = (user.demoBalance ?? 0).toFixed(2);


    /// <summary>
    /// Pulls the latest user record from the API and syncs balance email
    /// into the page and localStorage.
    /// </summary>
    async function refreshUserFromApi() {
        try {
            const res = await fetch(`/api/user/${userId}`);
            if (!res.ok) return;

            const data = await res.json();
            balanceSpan.textContent = (data.demoBalance ?? 0).toFixed(2);
            emailSpan.textContent = data.email;
            localStorage.setItem("user", JSON.stringify(data));
        } catch (err) {
            console.error("Error refreshing user:", err);
        }
    }

    if (refreshBalanceBtn) {
        refreshBalanceBtn.addEventListener("click", async () => {
            await refreshUserFromApi();
        });
    }

    // --- Change Email: open modal ---
    if (changeEmailBtn) {
        changeEmailBtn.addEventListener("click", () => {
            const newEmailInput = document.getElementById("newEmail");
            const currentPwInput = document.getElementById("currentPasswordForEmail");
            const msg = document.getElementById("update-email-message");

            if (newEmailInput) newEmailInput.value = "";
            if (currentPwInput) currentPwInput.value = "";
            if (msg) {
                msg.textContent = "";
                msg.classList.remove("text-danger", "text-success");
            }

            changeEmailModal.show();
        });
    }

    /// <summary>
    /// Loads all saved payment methods for the user and renders them
    /// as a styled list inside the "Payment Methods" modal.
    /// </summary>
    async function loadPaymentMethods() {
        paymentMethodsContainer.innerHTML =
            `<p class="text-muted mb-0">Loading payment methods...</p>`;

        try {
            const res = await fetch(`/api/paymentmethod/user/${userId}`);
            if (!res.ok) {
                paymentMethodsContainer.innerHTML =
                    `<p class="text-danger mb-0">Failed to load payment methods.</p>`;
                return;
            }

            const methods = await res.json();

            if (!methods || methods.length === 0) {
                paymentMethodsContainer.innerHTML =
                    `<p class="text-warning mb-0">No payment methods saved yet.</p>`;
                return;
            }

            let html = `<ul class="list-group list-group-flush">`;

            for (const m of methods) {
                // Safely normalize nickname + other fields
                const rawNickname = m.nickname;
                const nickname = (typeof rawNickname === "string" ? rawNickname : "").trim();

                const holder = (typeof m.cardholderName === "string"
                    ? m.cardholderName
                    : "Cardholder");

                const masked = (typeof m.maskedCardNumber === "string"
                    ? m.maskedCardNumber
                    : "**** **** **** ****");

                const expMonth = m.expMonth ?? "--";
                const expYear = m.expYear ?? "----";
                const isActive = m.isActive === true;

                const displayName = nickname.length > 0 ? nickname : holder;

                html += `
<li class="list-group-item d-flex justify-content-between align-items-center bg-dark text-white">
    <div>
        <div><strong>${displayName}</strong></div>
        <div class="small text-muted">${holder}</div>
        <div class="small text-muted">${masked}</div>
        <div class="small text-muted">Exp: ${expMonth.toString().padStart(2, "0")} / ${expYear}</div>
        <div class="small ${isActive ? "text-success" : "text-danger"}">
            ${isActive ? "Active" : "Inactive"}
        </div>
    </div>
    <button class="btn btn-sm btn-outline-danger" onclick="deletePaymentMethod(${m.id})">
        Remove
    </button>
</li>`;
            }

            html += `</ul>`;
            paymentMethodsContainer.innerHTML = html;

        } catch (err) {
            console.error(err);
            paymentMethodsContainer.innerHTML =
                `<p class="text-danger mb-0">Error loading payment methods.</p>`;
        }
    }

    /// <summary>
    /// Fills the "Add Funds" dropdown with only active payment
    /// methods, so the user can pick which card they are “using”.
    /// </summary>
    async function populateDepositMethods() {
        const select = document.getElementById("deposit-payment-method");
        if (!select) return;

        // Clear out anything currently there
        select.innerHTML = "";

        try {
            const res = await fetch(`/api/paymentmethod/user/${userId}`);
            if (!res.ok) {
                select.innerHTML = `<option disabled selected>Failed to load methods</option>`;
                return;
            }

            const methods = await res.json();
            const active = Array.isArray(methods)
                ? methods.filter(m => m.isActive === true)
                : [];

            if (active.length === 0) {
                select.innerHTML = `<option disabled selected>No active payment methods</option>`;
                return;
            }

            // Add options
            let first = true;
            for (const m of active) {
                const nickname = (typeof m.nickname === "string" ? m.nickname : "").trim();
                const holder = (typeof m.cardholderName === "string"
                    ? m.cardholderName
                    : "Cardholder");
                const masked = (typeof m.maskedCardNumber === "string"
                    ? m.maskedCardNumber
                    : "**** **** **** ****");
                const displayName = nickname.length > 0 ? nickname : holder;

                const opt = document.createElement("option");
                opt.value = String(m.id);
                opt.textContent = `${displayName} (${masked})`;
                if (first) {
                    opt.selected = true;
                    first = false;
                }

                select.appendChild(opt);
            }
        } catch (err) {
            console.error("Error loading deposit methods:", err);
            select.innerHTML = `<option disabled selected>Error loading methods</option>`;
        }
    }

    /// <summary>
    /// Global function used by the inline "Remove" button.
    /// Sends a DELETE request and refreshes the payment method list.
    /// </summary>
    window.deletePaymentMethod = async function (id) {
        if (!confirm("Remove this payment method?")) return;

        try {
            const res = await fetch(`/api/paymentmethod/${id}`, {
                method: "DELETE"
            });

            if (!res.ok) {
                alert("Failed to delete payment method.");
                return;
            }

            await loadPaymentMethods();

        } catch (err) {
            console.error(err);
            alert("Error removing payment method.");
        }
    };

    // --- Load methods when payment modal opens ---
    if (paymentMethodsModalEl) {
        paymentMethodsModalEl.addEventListener("show.bs.modal", async () => {
            await loadPaymentMethods();
        });
    }

    // --- Add payment method form ---
    const addPaymentMethodForm = document.getElementById("add-payment-method-form");
    const addPaymentMethodMsg = document.getElementById("add-payment-method-message");

    /// <summary>
    /// Handles submit for the "Add Payment Method" form:
    /// validates basic fields, hits the API, shows a status message,
    /// and reloads the list in the modal.
    /// </summary>
    if (addPaymentMethodForm) {
        addPaymentMethodForm.addEventListener("submit", async (e) => {
            e.preventDefault();
            if (addPaymentMethodMsg) {
                addPaymentMethodMsg.textContent = "";
                addPaymentMethodMsg.classList.remove("text-danger", "text-success");
            }

            const cardholderNameEl = document.getElementById("cardholderName");
            const cardNumberEl = document.getElementById("cardNumber");
            const expMonthEl = document.getElementById("expMonth");
            const expYearEl = document.getElementById("expYear");
            const cvvEl = document.getElementById("cvv");
            const nicknameEl = document.getElementById("nickname");

            const cardholderName = cardholderNameEl ? cardholderNameEl.value : "";
            const cardNumber = cardNumberEl
                ? cardNumberEl.value.replace(/\s|-/g, "")
                : "";
            const expMonth = expMonthEl ? expMonthEl.value : "";
            const expYear = expYearEl ? expYearEl.value : "";
            const cvv = cvvEl ? cvvEl.value : "";
            const nickname = nicknameEl ? nicknameEl.value : "";

            const body = new URLSearchParams({
                CardholderName: cardholderName,
                CardNumber: cardNumber,
                ExpMonth: expMonth,
                ExpYear: expYear,
                Cvv: cvv,
                Nickname: nickname
            });

            try {
                const res = await fetch(`/api/paymentmethod/user/${userId}`, {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/x-www-form-urlencoded"
                    },
                    body: body.toString()
                });

                if (!res.ok) {
                    let msgText = "Failed to add payment method.";
                    if (res.status === 400) {
                        msgText = "Card info is invalid or expired.";
                    }
                    if (addPaymentMethodMsg) {
                        addPaymentMethodMsg.classList.add("text-danger");
                        addPaymentMethodMsg.textContent = msgText;
                    }
                    return;
                }

                if (addPaymentMethodMsg) {
                    addPaymentMethodMsg.classList.add("text-success");
                    addPaymentMethodMsg.textContent = "Payment method added.";
                }

                addPaymentMethodForm.reset();
                await loadPaymentMethods();

                setTimeout(() => {
                    addPaymentMethodModal.hide();
                }, 600);

            } catch (err) {
                console.error(err);
                if (addPaymentMethodMsg) {
                    addPaymentMethodMsg.classList.add("text-danger");
                    addPaymentMethodMsg.textContent = "Error adding payment method.";
                }
            }
        });
    }

    // --- Add Funds: modal show hook ---
    if (addFundsModalEl) {
        addFundsModalEl.addEventListener("show.bs.modal", async () => {
            // Reset form + message
            const form = document.getElementById("add-funds-form");
            const msg = document.getElementById("add-funds-message");
            if (form) form.reset();
            if (msg) {
                msg.textContent = "";
                msg.classList.remove("text-danger", "text-success");
            }

            await populateDepositMethods();
        });
    }

    // --- Add Funds form submit ---
    const addFundsForm = document.getElementById("add-funds-form");
    const addFundsMsg = document.getElementById("add-funds-message");


    /// <summary>
    /// Handles wallet deposits using fake credits
    /// validates the amount, calls /api/wallet/deposit,
    /// updates the on-screen balance and localStorage.
    /// </summary>
    if (addFundsForm) {
        addFundsForm.addEventListener("submit", async (e) => {
            e.preventDefault();
            if (addFundsMsg) {
                addFundsMsg.textContent = "";
                addFundsMsg.classList.remove("text-danger", "text-success");
            }

            const amountRaw = document.getElementById("deposit-amount")?.value ?? "";
            const description = document.getElementById("deposit-description")?.value ?? "";

            const amount = parseFloat(amountRaw);
            if (isNaN(amount) || amount <= 0) {
                if (addFundsMsg) {
                    addFundsMsg.classList.add("text-danger");
                    addFundsMsg.textContent = "Please enter a valid positive amount.";
                }
                return;
            }

            const body = new URLSearchParams({
                UserId: String(userId),
                Amount: amount.toString(),
                Description: description
            });

            try {
                const res = await fetch("/api/wallet/deposit", {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/x-www-form-urlencoded"
                    },
                    body: body.toString()
                });

                if (!res.ok) {
                    if (addFundsMsg) {
                        addFundsMsg.classList.add("text-danger");
                        addFundsMsg.textContent = "Failed to add funds.";
                    }
                    return;
                }

                const data = await res.json();

                // Update balance on screen
                const newBalance = data.newBalance ?? data.NewBalance ?? null;
                if (typeof newBalance === "number") {
                    balanceSpan.textContent = newBalance.toFixed(2);
                    // Update localStorage user cache
                    user.demoBalance = newBalance;
                    localStorage.setItem("user", JSON.stringify(user));
                } else {
                    // Fallback refresh from API if shape is weird
                    await refreshUserFromApi();
                }

                if (addFundsMsg) {
                    addFundsMsg.classList.add("text-success");
                    addFundsMsg.textContent = "Funds added successfully.";
                }

                setTimeout(() => {
                    if (addFundsModal) {
                        addFundsModal.hide();
                    }
                }, 700);

            } catch (err) {
                console.error("Error adding funds:", err);
                if (addFundsMsg) {
                    addFundsMsg.classList.add("text-danger");
                    addFundsMsg.textContent = "Error adding funds.";
                }
            }
        });
    }

    // --- Change Email form ---
    const changeEmailForm = document.getElementById("change-email-form");
    const updateEmailMsg = document.getElementById("update-email-message");


    /// <summary>
    /// Submits an email change request with the current password
    /// for verification, then refreshes the cached user info.
    /// </summary>
    if (changeEmailForm) {
        changeEmailForm.addEventListener("submit", async (e) => {
            e.preventDefault();
            if (updateEmailMsg) {
                updateEmailMsg.textContent = "";
                updateEmailMsg.classList.remove("text-danger", "text-success");
            }

            const newEmail = document.getElementById("newEmail")?.value ?? "";
            const currentPassword = document.getElementById("currentPasswordForEmail")?.value ?? "";

            const body = new URLSearchParams({
                NewEmail: newEmail,
                CurrentPassword: currentPassword
            });

            try {
                const res = await fetch(`/api/user/${userId}/email`, {
                    method: "PUT",
                    headers: {
                        "Content-Type": "application/x-www-form-urlencoded"
                    },
                    body: body.toString()
                });

                if (res.status === 401) {
                    if (updateEmailMsg) {
                        updateEmailMsg.classList.add("text-danger");
                        updateEmailMsg.textContent = "Current password is incorrect.";
                    }
                    return;
                }

                if (res.status === 409) {
                    if (updateEmailMsg) {
                        updateEmailMsg.classList.add("text-danger");
                        updateEmailMsg.textContent = "That email is already in use.";
                    }
                    return;
                }

                if (!res.ok) {
                    if (updateEmailMsg) {
                        updateEmailMsg.classList.add("text-danger");
                        updateEmailMsg.textContent = "Failed to update email.";
                    }
                    return;
                }

                if (updateEmailMsg) {
                    updateEmailMsg.classList.add("text-success");
                    updateEmailMsg.textContent = "Email updated successfully.";
                }

                await refreshUserFromApi();
                changeEmailForm.reset();

                setTimeout(() => {
                    changeEmailModal.hide();
                }, 600);

            } catch (err) {
                console.error(err);
                if (updateEmailMsg) {
                    updateEmailMsg.classList.add("text-danger");
                    updateEmailMsg.textContent = "Error updating email.";
                }
            }
        });
    }

    // --- Change Password form ---
    const changePasswordForm = document.getElementById("change-password-form");
    const updatePasswordMsg = document.getElementById("update-password-message");


    /// <summary>
    /// Sends a password change request after checking the current
    /// password, shows a short success or error message.
    /// </summary>
    if (changePasswordForm) {
        changePasswordForm.addEventListener("submit", async (e) => {
            e.preventDefault();
            if (updatePasswordMsg) {
                updatePasswordMsg.textContent = "";
                updatePasswordMsg.classList.remove("text-danger", "text-success");
            }

            const currentPassword = document.getElementById("currentPassword")?.value ?? "";
            const newPassword = document.getElementById("newPassword")?.value ?? "";

            const body = new URLSearchParams({
                CurrentPassword: currentPassword,
                NewPassword: newPassword
            });

            try {
                const res = await fetch(`/api/user/${userId}/password`, {
                    method: "PUT",
                    headers: {
                        "Content-Type": "application/x-www-form-urlencoded"
                    },
                    body: body.toString()
                });

                if (res.status === 401) {
                    if (updatePasswordMsg) {
                        updatePasswordMsg.classList.add("text-danger");
                        updatePasswordMsg.textContent = "Current password is incorrect.";
                    }
                    return;
                }

                if (!res.ok) {
                    if (updatePasswordMsg) {
                        updatePasswordMsg.classList.add("text-danger");
                        updatePasswordMsg.textContent = "Failed to update password.";
                    }
                    return;
                }

                if (updatePasswordMsg) {
                    updatePasswordMsg.classList.add("text-success");
                    updatePasswordMsg.textContent = "Password updated successfully.";
                }

                changePasswordForm.reset();

                setTimeout(() => {
                    changePasswordModal.hide();
                }, 600);

            } catch (err) {
                console.error(err);
                if (updatePasswordMsg) {
                    updatePasswordMsg.classList.add("text-danger");
                    updatePasswordMsg.textContent = "Error updating password.";
                }
            }
        });
    }
});