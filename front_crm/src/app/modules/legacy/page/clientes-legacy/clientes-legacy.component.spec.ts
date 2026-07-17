import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClientTesting } from '@angular/common/http/testing';

import { ClientesLegacyComponent } from './clientes-legacy.component';

describe('ClientesLegacyComponent', () => {
  let component: ClientesLegacyComponent;
  let fixture: ComponentFixture<ClientesLegacyComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      providers: [provideHttpClientTesting()],
imports: [ClientesLegacyComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ClientesLegacyComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
