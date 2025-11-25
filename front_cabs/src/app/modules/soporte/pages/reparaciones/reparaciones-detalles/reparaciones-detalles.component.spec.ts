import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ReparacionesDetallesComponent } from './reparaciones-detalles.component';

describe('ReparacionesDetallesComponent', () => {
  let component: ReparacionesDetallesComponent;
  let fixture: ComponentFixture<ReparacionesDetallesComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ReparacionesDetallesComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ReparacionesDetallesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
