// Claves usadas para guardar datos de sesión en localStorage.
// Las centralizamos para evitar strings repetidos en varios archivos.
const TOKEN_STORAGE_KEY = "kim_scheduler_token";
const USER_STORAGE_KEY = "kim_scheduler_user";

// Lee el JWT guardado después del login.
// Si devuelve null, significa que el usuario no ha iniciado sesión.
export function getAuthToken(): string | null {
  return localStorage.getItem(TOKEN_STORAGE_KEY);
}

// Revisa si existe una sesión local.
// Importante: esto NO valida si el token está vencido.
// La validación real la hace el backend con [Authorize].
export function isAuthenticated(): boolean {
  return getAuthToken() !== null;
}

// Construye los headers para endpoints protegidos.
// ASP.NET Core espera el token en:
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

// Guarda el token y los datos básicos del usuario.
// No guardamos información sensible como password o PasswordHash.
export function saveAuthSession(token: string, user: unknown): void {
  localStorage.setItem(TOKEN_STORAGE_KEY, token);
  localStorage.setItem(USER_STORAGE_KEY, JSON.stringify(user));
}

// Elimina la sesión local.
// Después de esto, el frontend ya no tendrá token para endpoints protegidos.
export function clearAuthSession(): void {
  localStorage.removeItem(TOKEN_STORAGE_KEY);
  localStorage.removeItem(USER_STORAGE_KEY);
}
