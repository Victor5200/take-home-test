import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { LoanService } from './services/loan.service';
import { Loan } from './models/loan.model';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatButtonModule, MatProgressSpinnerModule],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
})
export class AppComponent implements OnInit {
  displayedColumns: string[] = [
    'loanAmount',
    'currentBalance',
    'applicant',
    'status',
  ];
  loans: Loan[] = [];
  loading = false;
  error: string | null = null;

  constructor(private loanService: LoanService) {}

  ngOnInit(): void {
    this.loadLoans();
  }

  loadLoans(): void {
    this.loading = true;
    this.error = null;

    this.loanService.getLoans().subscribe({
      next: (data) => {
        this.loans = data;
        this.loading = false;
      },
      error: (error) => {
        this.error = 'Failed to load loans. Please make sure the backend API is running.';
        this.loading = false;
        console.error('Error loading loans:', error);
      }
    });
  }
}
