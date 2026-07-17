import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClientTesting } from '@angular/common/http/testing';

import { MetricasDocumentosComponent } from './metricas-documentos.component';

describe('MetricasDocumentosComponent', () => {
  let component: MetricasDocumentosComponent;
  let fixture: ComponentFixture<MetricasDocumentosComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      providers: [provideHttpClientTesting()],
imports: [MetricasDocumentosComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MetricasDocumentosComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
