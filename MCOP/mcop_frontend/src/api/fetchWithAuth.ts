interface FetchOptions extends RequestInit {
  headers?: Record<string, string>;
}

async function fetchWithAuth<T = any>(url: string, options: FetchOptions = {}): Promise<T> {
  const token = localStorage.getItem('app_session');
  
  const headers: Record<string, string> = {
    ...options.headers,
  };

  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  }

  const response = await fetch(url, {
    ...options,
    headers,
  });

  if (!response.ok) {
    throw new Error(`HTTP error! status: ${response.status}`);
  }

  const contentType = response.headers.get('content-type');
  if (!contentType || !contentType.includes('application/json')) {
    return null as T;
  }

  return response.json();
}

export default fetchWithAuth;