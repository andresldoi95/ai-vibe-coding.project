import type { CertificateInfo, SriConfiguration, UpdateSriConfigurationDto, UploadCertificateDto } from '~/types/sri-configuration'

interface ApiResponse<T> {
  data: T
  success: boolean
}

export function useSriConfiguration() {
  const { apiFetch } = useApi()

  async function getSriConfiguration(): Promise<SriConfiguration> {
    const response = await apiFetch<ApiResponse<SriConfiguration>>('/sri-configuration', {
      method: 'GET',
    })
    return response.data
  }

  async function updateSriConfiguration(data: UpdateSriConfigurationDto): Promise<SriConfiguration> {
    const response = await apiFetch<ApiResponse<SriConfiguration>>('/sri-configuration', {
      method: 'PUT',
      body: data,
    })
    return response.data
  }

  async function uploadCertificate(data: UploadCertificateDto): Promise<SriConfiguration> {
    const formData = new FormData()
    formData.append('certificateFile', data.certificateFile)
    formData.append('certificatePassword', data.certificatePassword)

    const response = await apiFetch<ApiResponse<SriConfiguration>>('/sri-configuration/certificate', {
      method: 'POST',
      body: formData,
    })
    return response.data
  }

  async function getCertificateInfo(): Promise<CertificateInfo | null> {
    try {
      const response = await apiFetch<ApiResponse<CertificateInfo>>('/sri-configuration/certificate/info', {
        method: 'GET',
      })
      return response.data
    }
    catch {
      return null
    }
  }

  async function removeCertificate(): Promise<void> {
    await apiFetch('/sri-configuration/certificate', {
      method: 'DELETE',
    })
  }

  return {
    getSriConfiguration,
    updateSriConfiguration,
    uploadCertificate,
    getCertificateInfo,
    removeCertificate,
  }
}
