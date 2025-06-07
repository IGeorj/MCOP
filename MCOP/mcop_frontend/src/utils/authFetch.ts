import { config } from "../config";

type ResponseType = 'json' | 'blob' | 'text' | 'arrayBuffer' | 'formData';

export async function authFetch<T>(
    url: string,
    options: {
        responseType?: ResponseType;
        requestInit?: RequestInit;
    } = {}
): Promise<T> {
    const token = localStorage.getItem("app_session");

    const headers = new Headers(options.requestInit?.headers);
    headers.set('Authorization', `Bearer ${token}`);

    if (options.responseType !== 'blob') {
        headers.set('Content-Type', 'application/json');
    }

    const response = await fetch(`${config.API_URL}${url}`, {
        ...options.requestInit,
        headers,
    });

    if (!response.ok) {
        const error = await tryGetError(response);
        throw error;
    }

    switch (options.responseType) {
        case 'blob':
            return response.blob() as Promise<T>;
        case 'text':
            return response.text() as Promise<T>;
        case 'arrayBuffer':
            return response.arrayBuffer() as Promise<T>;
        case 'formData':
            return response.formData() as Promise<T>;
        case 'json':
        default:
            return response.json() as Promise<T>;
    }
}

async function tryGetError(response: Response): Promise<Error> {
    try {
        const errorData = await response.json();
        return new Error(errorData.message || `Request failed with status ${response.status}`);
    } catch {
        return new Error(`Request failed with status ${response.status}`);
    }
}