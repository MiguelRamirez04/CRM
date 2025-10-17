import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BandejaPerfilComponent } from './bandeja-perfil.component';

describe('BandejaPerfil', () => {
  let component: BandejaPerfilComponent;
  let fixture: ComponentFixture<BandejaPerfilComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BandejaPerfilComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(BandejaPerfilComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
