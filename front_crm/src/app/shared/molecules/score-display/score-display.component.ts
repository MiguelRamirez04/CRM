import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-score-display',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './score-display.component.html',
  styleUrls: ['./score-display.component.css']
})
export class ScoreDisplayComponent implements OnChanges {
  @Input() label: string = 'Puntuación';
  @Input() score: number = 0;
  @Input() maxScore: number = 100;
  @Input() showMaxScore: boolean = true;
  @Input() size: 'small' | 'medium' | 'large' = 'medium';
  @Input() readonly: boolean = true;

  // ✅ Propiedades normales en lugar de getters
  scorePercentage: number = 0;
  scoreColor: string = 'error';

  ngOnChanges(changes: SimpleChanges): void {
    // Solo recalcula cuando score o maxScore cambian
    if (changes['score'] || changes['maxScore']) {
      this.calculateScore();
    }
  }

  private calculateScore(): void {
    this.scorePercentage = (this.score / this.maxScore) * 100;
    
    if (this.scorePercentage >= 80) {
      this.scoreColor = 'success';
    } else if (this.scorePercentage >= 60) {
      this.scoreColor = 'warning';
    } else {
      this.scoreColor = 'error';
    }
  }
}