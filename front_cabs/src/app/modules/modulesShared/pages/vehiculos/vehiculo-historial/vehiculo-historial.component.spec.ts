import { ComponentFixture, TestBed } from '@angular/core/testing';

import { VehiculoHistorialComponent } from './vehiculo-historial.component';

describe('VehiculoHistorialComponent', () => {
  let component: VehiculoHistorialComponent;
  let fixture: ComponentFixture<VehiculoHistorialComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VehiculoHistorialComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(VehiculoHistorialComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
