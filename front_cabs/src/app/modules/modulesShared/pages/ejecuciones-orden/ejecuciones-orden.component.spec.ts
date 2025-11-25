import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EjecucionesOrdenComponent } from './ejecuciones-orden.component';

describe('EjecucionesOrdenComponent', () => {
  let component: EjecucionesOrdenComponent;
  let fixture: ComponentFixture<EjecucionesOrdenComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EjecucionesOrdenComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EjecucionesOrdenComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
