export interface Loan {
  id: number;
  amount: number;
  currentBalance: number;
  applicantName: string;
  status: 'active' | 'paid';
  createdAt: string;
  updatedAt?: string;
}

export interface CreateLoanDto {
  amount: number;
  applicantName: string;
}

export interface PaymentDto {
  amount: number;
}
