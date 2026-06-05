// Claves usadas en localStorage.
// Las centralizamos para no escribir strings sueltos por toda la app.
const TOKEN_STORAGE_KEY = "kim_scheduler_token";
const USER_STORAGE_KEY = "kim_scheduler_user";

// Lee el JWT guardado después del login.
// Si no existe, significa que el usuario no ha iniciado sesión.
export function getAuthToken(): string | null {
  return localStorage.getItem(TOKEN_STORAGE_KEY);
}

// Devuelve true si existe un token guardado.
// Ojo: esto solo revisa si hay token, no valida si está vencido.
// La validación real la hace el backend con [Authorize].
export function isAuthenticated(): boolean {
  return getAuthToken() !== null;
}

// Construye los headers necesarios para endpoints protegidos.
// ASP.NET Core espera el JWT en:
// Authorization: Bearer {token}
export function getAuthHeaders(): HeadersInit {
  const token = getAuthToken();

  if (!token) {
    return {
      "Content-Type": "application/json",
    };
  }

  return {
    "Content-Type": "application/json",
    Authorization: `Bearer ${token}`,
  };
}

// Guarda el token y datos básicos del usuario después del login.
// No guardamos información sensible como PasswordHash.
export function saveAuthSession(token: string, user: unknown): void {
  localStorage.setItem(TOKEN_STORAGE_KEY, token);
  localStorage.setItem(USER_STORAGE_KEY, JSON.stringify(user));
}

// Elimina la sesión local del navegador.
// Después de esto, el usuario ya no puede usar acciones protegidas desde el frontend.
export function clearAuthSession(): void {
  localStorage.removeItem(TOKEN_STORAGE_KEY);
  localStorage.removeItem(USER_STORAGE_KEY);
}
