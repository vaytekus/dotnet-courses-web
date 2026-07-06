function togglePassword(buttonId, inputId) {
    const button = document.getElementById(buttonId);
    const input = document.getElementById(inputId);
    if (!button || !input) return;

    button.innerHTML = '<i class="bi bi-eye"></i>';

    button.addEventListener('click', function () {
        const isHidden = input.type === 'password';
        input.type = isHidden ? 'text' : 'password';
        button.innerHTML = isHidden
            ? '<i class="bi bi-eye-slash"></i>'
            : '<i class="bi bi-eye"></i>';
    });
}

togglePassword('toggle-login-password', 'login-password');
togglePassword('toggle-register-password', 'register-password');
togglePassword('toggle-register-confirm-password', 'register-confirm-password');