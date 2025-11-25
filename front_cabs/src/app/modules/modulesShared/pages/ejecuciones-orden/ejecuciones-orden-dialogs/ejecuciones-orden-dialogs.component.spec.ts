import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EjecucionesOrdenDialogsComponent } from './ejecuciones-orden-dialogs.component';

describe('EjecucionesOrdenDialogsComponent', () => {
  let component: EjecucionesOrdenDialogsComponent;
  let fixture: ComponentFixture<EjecucionesOrdenDialogsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EjecucionesOrdenDialogsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EjecucionesOrdenDialogsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
