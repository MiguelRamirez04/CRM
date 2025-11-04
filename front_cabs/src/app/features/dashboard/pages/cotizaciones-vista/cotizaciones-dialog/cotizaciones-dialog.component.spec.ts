import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CotizacionesDialogComponent } from './cotizaciones-dialog.component';

describe('CotizacionesDialogComponent', () => {
  let component: CotizacionesDialogComponent;
  let fixture: ComponentFixture<CotizacionesDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CotizacionesDialogComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CotizacionesDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
