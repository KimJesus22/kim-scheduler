import { API_URL, requestJson } from "../../lib/api";
import { getAuthHeaders } from "../../lib/auth";

// Tipo que representa un negocio devuelto por el backend.
// Lo usamos para pintar tarjetas visuales en el playground.
export type Business = {
  id: string;
  name: string;
  slug: string;
  phone: string | null;
  email: string | null;
  isActive: boolean;
  createdAtUtc: string;
};

// Renderiza la lista visual de negocios activos.
// Recibe el contenedor HTML donde se van a pintar las tarjetas.
export function renderBusinesses(
  businessesList: HTMLDivElement | null,
  businesses: Business[]
): void {
  if (!businessesList) return;

  if (businesses.length === 0) {
    businessesList.innerHTML = `
      <p class="text-sm text-[var(--ks-text-muted)]">
        No hay negocios activos todavía.
      </p>
    `;
    return;
  }

  businessesList.innerHTML = businesses
    .map(
      (business) => `
        <article class="rounded-2xl border border-[var(--ks-border)] bg-[var(--ks-surface-soft)] p-4">
          <div class="flex items-start justify-between gap-3">
            <div>
              <h3 class="font-bold text-[var(--ks-text)]">
                ${business.name}
              </h3>

              <p class="mt-1 text-sm text-[var(--ks-text-muted)]">
                /${business.slug}
              </p>
            </div>

            <span class="rounded-full bg-[var(--ks-primary-soft)] px-3 py-1 text-xs font-bold text-[var(--ks-primary)]">
              Activo
            </span>
          </div>

          <div class="mt-4 space-y-1 text-sm text-[var(--ks-text-muted)]">
            <p>Tel: ${business.phone ?? "Sin teléfono"}</p>
            <p>Email: ${business.email ?? "Sin email"}</p>
          </div>

          <button
            class="deactivate-business-button mt-4 w-full rounded-full bg-[var(--ks-accent-soft)] px-4 py-2 text-sm font-bold text-[var(--ks-accent)] hover:bg-[var(--ks-surface)]"
            data-business-id="${business.id}"
          >
            Desactivar negocio
          </button>
        </article>
      `
    )
    .join("");
}

// Obtiene negocios activos desde el backend y los pinta en pantalla.
// También devuelve el resultado para que playground pueda mostrarlo en el panel JSON.
export async function loadAndRenderBusinesses(
  businessesList: HTMLDivElement | null
) {
  const result = await requestJson(`${API_URL}/api/businesses`);

  if (Array.isArray(result.data)) {
    renderBusinesses(businessesList, result.data as Business[]);
  }

  return result;
}

// Conecta los botones "Desactivar negocio" creados dinámicamente.
// Como las tarjetas se generan con innerHTML, esta función debe llamarse
// después de renderBusinesses().
export function bindDeactivateBusinessButtons(
  businessesList: HTMLDivElement | null,
  showResult: (data: unknown) => void
): void {
  const deactivateButtons = document.querySelectorAll<HTMLButtonElement>(
    ".deactivate-business-button"
  );

  deactivateButtons.forEach((button) => {
    button.addEventListener("click", async () => {
      const businessId = button.dataset.businessId;

      if (!businessId) {
        showResult({
          ok: false,
          message: "No se encontró el ID del negocio.",
        });

        return;
      }

      try {
        // Este endpoint está protegido con rol Admin.
        const result = await requestJson(
          `${API_URL}/api/businesses/${businessId}/deactivate`,
          {
            method: "PATCH",
            headers: getAuthHeaders(),
          }
        );

        showResult(result);

        // Después de desactivar, recargamos la lista visual.
        const businessesResult = await loadAndRenderBusinesses(businessesList);
        showResult(businessesResult);

        // Como la lista se volvió a renderizar, reconectamos botones nuevos.
        bindDeactivateBusinessButtons(businessesList, showResult);
      } catch {
        showResult({
          ok: false,
          message: "No se pudo desactivar el negocio.",
        });
      }
    });
  });
}
