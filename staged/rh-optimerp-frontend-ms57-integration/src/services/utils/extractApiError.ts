export const extractApiError = (err: unknown, fallback = 'Erreur'): string => {
  const response = (err as { response?: { data?: { error?: string } } })?.response;
  return response?.data?.error || fallback;
};
