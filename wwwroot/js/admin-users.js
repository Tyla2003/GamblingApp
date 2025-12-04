"use strict";

document.addEventListener("DOMContentLoaded", () => {
    const userJson = localStorage.getItem("user");
    const adminWrapper = document.getElementById("admin-wrapper");
    const adminDenied = document.getElementById("admin-denied");

    if (!userJson) {
        alert("You must be logged in to view this page.");
        window.location.href = "/Account/Login";
        return;
    }

    const currentUser = JSON.parse(userJson);

    // Gate access by admin flag
    const raw = currentUser.isAdmin;
    const isAdmin =
        raw === true ||
        raw === 1 ||
        raw === "1" ||
        raw === "True" ||
        raw === "true";

    if (!isAdmin) {
        adminDenied.style.display = "block";
        return;
    }

    // Show admin UI
    adminWrapper.style.display = "block";

    // Header info
    document.getElementById("admin-name").textContent =
        `${currentUser.firstName} ${currentUser.lastName}`;
    document.getElementById("admin-email").textContent = currentUser.email;

    // Table and controls
    const usersTableBody = document.getElementById("users-table-body");
    const searchInput = document.getElementById("user-search");
    const refreshBtn = document.getElementById("refresh-users");

    // User details modal + admin action controls
    const userDetailsModalEl = document.getElementById("userDetailsModal");
    const userDetailsModal = new bootstrap.Modal(userDetailsModalEl);

    const detailId = document.getElementById("detail-id");
    const detailName = document.getElementById("detail-name");
    const detailEmail = document.getElementById("detail-email");
    const detailBalance = document.getElementById("detail-balance");
    const detailIsAdmin = document.getElementById("detail-isadmin");
    const detailCreated = document.getElementById("detail-created");

    const actionsMsg = document.getElementById("admin-actions-message");
    const btnAddCredits = document.getElementById("btn-admin-add-credits");
    const btnChangeEmail = document.getElementById("btn-admin-change-email");
    const btnToggleAdmin = document.getElementById("btn-toggle-admin");
    const inputNewEmail = document.getElementById("admin-new-email");

    // Transactions modal + controls
    const viewTransactionsBtn = document.getElementById("view-transactions-btn");
    const userTransactionsModalEl = document.getElementById("userTransactionsModal");
    const txUserNameSpan = document.getElementById("tx-user-name");
    const txTableBody = document.getElementById("tx-table-body");
    const txSortField = document.getElementById("tx-sort-field");
    const txSortDirection = document.getElementById("tx-sort-direction");

    let userTransactionsModal = null;
    if (userTransactionsModalEl) {
        userTransactionsModal = new bootstrap.Modal(userTransactionsModalEl);
    }

    let allUsers = [];
    let selectedUser = null;   // user currently in details modal
    let currentTxList = [];    // last-loaded transactions for selected user

    // ========== Helpers ==========


    /// <summary>
    /// Shows a short status message above the admin actions
    /// (success, warning, error) by resetting the CSS classes.
    /// </summary>
    function setActionsMessage(text, cssClass) {
        actionsMsg.textContent = text || "";
        actionsMsg.className = "mb-3 small";
        if (cssClass) {
            actionsMsg.classList.add(cssClass);
        }
    }


    /// <summary>
    /// Enables/disables admin action buttons depending on which
    /// user is selected, and prevents an admin from editing themself.
    /// Also updates the toggle admin button text and style.
    /// </summary>
    function configureActionButtons() {
        if (!selectedUser) return;

        const isSelf = selectedUser.id === currentUser.id;

        // Default state
        btnAddCredits.disabled = isSelf;
        btnChangeEmail.disabled = isSelf;
        btnToggleAdmin.disabled = isSelf;
        inputNewEmail.disabled = isSelf;

        if (isSelf) {
            setActionsMessage(
                "You cannot modify your own balance, email, or admin role from the admin panel.",
                "text-warning"
            );
        } else {
            setActionsMessage("", "");
        }

        // Toggle admin button text + style
        if (selectedUser.isAdmin) {
            btnToggleAdmin.textContent = "Remove Admin";
            btnToggleAdmin.classList.remove("btn-success");
            btnToggleAdmin.classList.add("btn-outline-warning");
        } else {
            btnToggleAdmin.textContent = "Make Admin";
            btnToggleAdmin.classList.remove("btn-outline-warning");
            btnToggleAdmin.classList.add("btn-success");
        }
    }

    // ========== Load / render users ==========


    /// <summary>
    /// Fetches all users from the API and calls renderUsers to
    /// populate the admin table with optional text filtering.
    /// </summary>
    async function loadUsers() {
        usersTableBody.innerHTML = `
<tr>
  <td colspan="7" class="text-center text-muted">Loading users...</td>
</tr>`;

        try {
            const res = await fetch("/api/user");
            if (!res.ok) {
                usersTableBody.innerHTML = `
<tr>
  <td colspan="7" class="text-center text-danger">Failed to load users.</td>
</tr>`;
                return;
            }

            const data = await res.json();
            allUsers = Array.isArray(data) ? data : [];
            renderUsers(allUsers, searchInput.value);
        } catch (err) {
            console.error(err);
            usersTableBody.innerHTML = `
<tr>
  <td colspan="7" class="text-center text-danger">Error loading users.</td>
</tr>`;
        }
    }


    /// <summary>
    /// Renders the user table with a simple case-insensitive filter
    /// that matches against name or email. Also wires up each row’s
    /// “View” button to open the details modal.
    /// </summary>
    function renderUsers(users, filterText) {
        const filter = (filterText || "").toLowerCase();

        const filtered = users.filter(u => {
            if (!filter) return true;
            const name = `${u.firstName ?? ""} ${u.lastName ?? ""}`.toLowerCase();
            const email = (u.email ?? "").toLowerCase();
            return name.includes(filter) || email.includes(filter);
        });

        if (filtered.length === 0) {
            usersTableBody.innerHTML = `
<tr>
  <td colspan="7" class="text-center text-warning">No users found.</td>
</tr>`;
            return;
        }

        let html = "";
        for (const u of filtered) {
            const name = `${u.firstName ?? ""} ${u.lastName ?? ""}`.trim();
            const createdAt = u.createdAt
                ? new Date(u.createdAt).toLocaleString()
                : "N/A";

            html += `
<tr>
  <td>${u.id}</td>
  <td>${name || "(no name)"}</td>
  <td>${u.email ?? ""}</td>
  <td>$${(u.demoBalance ?? 0).toFixed(2)}</td>
  <td>${u.isAdmin ? "Yes" : "No"}</td>
  <td>${createdAt}</td>
  <td class="text-end">
    <button class="btn btn-sm btn-outline-light" data-user-id="${u.id}">
      View
    </button>
  </td>
</tr>`;
        }

        usersTableBody.innerHTML = html;

        /// Hook up each View button to open the details modal for that row
        usersTableBody.querySelectorAll("button[data-user-id]")
            .forEach(btn => {
                btn.addEventListener("click", () => {
                    const id = parseInt(btn.getAttribute("data-user-id"), 10);
                    const user = allUsers.find(x => x.id === id);
                    if (!user) return;

                    selectedUser = { ...user }; // shallow copy

                    detailId.textContent = selectedUser.id;
                    detailName.textContent =
                        `${selectedUser.firstName ?? ""} ${selectedUser.lastName ?? ""}`.trim();
                    detailEmail.textContent = selectedUser.email ?? "";
                    detailBalance.textContent = (selectedUser.demoBalance ?? 0).toFixed(2);
                    detailIsAdmin.textContent = selectedUser.isAdmin ? "Yes" : "No";
                    detailCreated.textContent = selectedUser.createdAt
                        ? new Date(selectedUser.createdAt).toLocaleString()
                        : "N/A";

                    inputNewEmail.value = "";
                    setActionsMessage("", "");
                    configureActionButtons();

                    userDetailsModal.show();
                });
            });
    }

    // ========== Admin actions ==========

    // Add $1000 credit
    btnAddCredits.addEventListener("click", async () => {
        if (!selectedUser || selectedUser.id === currentUser.id) return;

        setActionsMessage("Applying admin credit...", "text-info");

        const body = new URLSearchParams({
            UserId: selectedUser.id.toString(),
            Amount: "1000",
            Reason: "Admin panel credit"
        });

        try {
            const res = await fetch("/api/wallet/admin-credit", {
                method: "POST",
                headers: {
                    "Content-Type": "application/x-www-form-urlencoded"
                },
                body: body.toString()
            });

            if (!res.ok) {
                setActionsMessage("Failed to apply admin credit.", "text-danger");
                return;
            }

            const data = await res.json();
            const newBalance = data.newBalance ?? data.NewBalance ?? selectedUser.demoBalance + 1000;

            selectedUser.demoBalance = newBalance;
            detailBalance.textContent = newBalance.toFixed(2);

            setActionsMessage("Successfully added $1000 to this user.", "text-success");
            await loadUsers();
        } catch (err) {
            console.error(err);
            setActionsMessage("Error applying admin credit.", "text-danger");
        }
    });

    // Change email (admin override for another user)
    btnChangeEmail.addEventListener("click", async () => {
        if (!selectedUser || selectedUser.id === currentUser.id) return;

        const newEmail = (inputNewEmail.value || "").trim();
        if (!newEmail) {
            setActionsMessage("Please enter a new email address.", "text-warning");
            return;
        }

        setActionsMessage("Updating email...", "text-info");

        const body = new URLSearchParams({
            NewEmail: newEmail
        });

        try {
            const res = await fetch(`/api/user/${selectedUser.id}/admin-email`, {
                method: "PUT",
                headers: {
                    "Content-Type": "application/x-www-form-urlencoded"
                },
                body: body.toString()
            });

            if (res.status === 409) {
                setActionsMessage("That email is already in use.", "text-danger");
                return;
            }

            if (!res.ok) {
                setActionsMessage("Failed to update email.", "text-danger");
                return;
            }

            selectedUser.email = newEmail;
            detailEmail.textContent = newEmail;
            inputNewEmail.value = "";

            setActionsMessage("Email updated successfully.", "text-success");
            await loadUsers();
        } catch (err) {
            console.error(err);
            setActionsMessage("Error updating email.", "text-danger");
        }
    });

    // Toggle admin role for selected user 
    btnToggleAdmin.addEventListener("click", async () => {
        if (!selectedUser || selectedUser.id === currentUser.id) return;

        const targetIsAdmin = !selectedUser.isAdmin;
        setActionsMessage("Updating admin role...", "text-info");

        const body = new URLSearchParams({
            IsAdmin: targetIsAdmin.toString()
        });

        try {
            const res = await fetch(`/api/user/${selectedUser.id}/admin-role`, {
                method: "PUT",
                headers: {
                    "Content-Type": "application/x-www-form-urlencoded"
                },
                body: body.toString()
            });

            if (!res.ok) {
                setActionsMessage("Failed to update admin role.", "text-danger");
                return;
            }

            selectedUser.isAdmin = targetIsAdmin;
            detailIsAdmin.textContent = targetIsAdmin ? "Yes" : "No";
            configureActionButtons();

            setActionsMessage("Admin role updated.", "text-success");
            await loadUsers();
        } catch (err) {
            console.error(err);
            setActionsMessage("Error updating admin role.", "text-danger");
        }
    });

    // ========== Transactions – load & render ==========



    /// <summary>
    /// Loads all wallet transactions for a user and stores them in
    /// currentTxList so they can be sorted and rendered in the modal.
    /// </summary>
    async function loadTransactionsForUser(userId) {
        if (!txTableBody) return;

        txTableBody.innerHTML = `
<tr>
  <td colspan="5" class="text-center text-muted">
    Loading transactions...
  </td>
</tr>`;

        try {
            const res = await fetch(`/api/wallet/user/${userId}/transactions`);
            if (!res.ok) {
                txTableBody.innerHTML = `
<tr>
  <td colspan="5" class="text-center text-danger">
    Failed to load transactions.
  </td>
</tr>`;
                return;
            }

            const data = await res.json();
            currentTxList = Array.isArray(data) ? data : [];
            renderTransactionsTable();
        } catch (err) {
            console.error(err);
            txTableBody.innerHTML = `
<tr>
  <td colspan="5" class="text-center text-danger">
    Error loading transactions.
  </td>
</tr>`;
        }
    }



    /// <summary>
    /// Sorts the in-memory transaction list by the selected field
    /// (amount, type, betId, createdAt) and direction, then writes
    /// the rows into the transactions table body.
    /// </summary>
    function renderTransactionsTable() {
        if (!txTableBody) return;

        if (!currentTxList || currentTxList.length === 0) {
            txTableBody.innerHTML = `
<tr>
  <td colspan="5" class="text-center text-warning">
    No transactions found.
  </td>
</tr>`;
            return;
        }

        const field = txSortField?.value || "createdAt";
        const dir = txSortDirection?.value || "desc";

        const sorted = [...currentTxList].sort((a, b) => {
            let av, bv;

            switch (field) {
                case "amount":
                    av = Number(a.amount ?? 0);
                    bv = Number(b.amount ?? 0);
                    break;
                case "type":
                    av = (a.type ?? "").toString();
                    bv = (b.type ?? "").toString();
                    break;
                case "betId":
                    av = Number(a.betId ?? 0);
                    bv = Number(b.betId ?? 0);
                    break;
                case "createdAt":
                default:
                    av = a.createdAt ? new Date(a.createdAt).getTime() : 0;
                    bv = b.createdAt ? new Date(b.createdAt).getTime() : 0;
                    break;
            }

            if (av < bv) return dir === "asc" ? -1 : 1;
            if (av > bv) return dir === "asc" ? 1 : -1;
            return 0;
        });

        let html = "";
        for (const tx of sorted) {
            const created = tx.createdAt
                ? new Date(tx.createdAt).toLocaleString()
                : "N/A";

            const amountNumber = Number(tx.amount ?? 0);
            const amount = amountNumber.toFixed(2);

            const type = tx.type ?? "";
            const description = tx.description ?? "";
            const betId = tx.betId ?? "";

            html += `
<tr>
  <td>${created}</td>
  <td>${type}</td>
  <td>$${amount}</td>
  <td>${description}</td>
  <td>${betId || "-"}</td>
</tr>`;
        }

        txTableBody.innerHTML = html;
    }

    // ========== Wire events / initial load ==========

    // View Transactions button in details modal
    if (viewTransactionsBtn && userTransactionsModal) {
        viewTransactionsBtn.addEventListener("click", async () => {
            if (!selectedUser) return;

            const displayName =
                `${selectedUser.firstName ?? ""} ${selectedUser.lastName ?? ""}`.trim()
                || selectedUser.email
                || `User ${selectedUser.id}`;

            if (txUserNameSpan) {
                txUserNameSpan.textContent = displayName;
            }

            await loadTransactionsForUser(selectedUser.id);
            userTransactionsModal.show();
        });
    }

    // Resort transactions whenever the sort dropdowns change
    if (txSortField) {
        txSortField.addEventListener("change", () => renderTransactionsTable());
    }
    if (txSortDirection) {
        txSortDirection.addEventListener("change", () => renderTransactionsTable());
    }

    // Refresh user list + live search
    refreshBtn.addEventListener("click", () => loadUsers());
    searchInput.addEventListener("input", () => renderUsers(allUsers, searchInput.value));

    // Initial load
    loadUsers();
});