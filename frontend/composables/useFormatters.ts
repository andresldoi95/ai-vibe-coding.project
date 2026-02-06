export function useFormatters() {
  const { locale } = useI18n()

  const formatCurrency = (amount: number, currency = 'USD') => {
    return new Intl.NumberFormat(locale.value, {
      style: 'currency',
      currency,
    }).format(amount)
  }

  const formatDate = (
    date: string | Date,
    format: 'short' | 'long' = 'short',
  ) => {
    const options: Intl.DateTimeFormatOptions
      = format === 'short'
        ? { year: 'numeric', month: '2-digit', day: '2-digit' }
        : { year: 'numeric', month: 'long', day: 'numeric' }

    return new Intl.DateTimeFormat(locale.value, options).format(
      new Date(date),
    )
  }

  const formatNumber = (num: number) => {
    return new Intl.NumberFormat(locale.value).format(num)
  }

  const formatDateTime = (date: string | Date) => {
    return new Intl.DateTimeFormat(locale.value, {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit',
    }).format(new Date(date))
  }

  return { formatCurrency, formatDate, formatNumber, formatDateTime }
}
