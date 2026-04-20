export type SurveyStatus = 'Draft' | 'Active' | 'Closed'
export type SurveyAccessMode = 'Anonymous' | 'CodeByEmail' | 'RequiresLogin'

export interface OptionResponse {
  id: string
  text: string
  order: number
}

export interface QuestionResponse {
  id: string
  text: string
  order: number
  isRequired: boolean
  options: OptionResponse[]
}

export interface SurveyDetailResponse {
  id: string
  title: string
  description?: string
  status: SurveyStatus
  accessMode: SurveyAccessMode
  collectedFields: number
  isPublic: boolean
  startDate?: string
  endDate?: string
  questions: QuestionResponse[]
  createdAt: string
  updatedAt?: string
}

export interface SurveyResponse {
  id: string
  title: string
  description?: string
  status: SurveyStatus
  accessMode: SurveyAccessMode
  isPublic: boolean
  startDate?: string
  endDate?: string
  questionCount: number
  responseCount: number
  createdAt: string
  updatedAt?: string
}

export interface PaginatedResponse<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}

export interface OptionResultResponse {
  optionId: string
  optionText: string
  count: number
  percentage: number
}

export interface QuestionResultResponse {
  questionId: string
  questionText: string
  options: OptionResultResponse[]
}

export interface SurveyResultResponse {
  surveyId: string
  surveyTitle: string
  totalResponses: number
  questions: QuestionResultResponse[]
}
