/** JWT de prueba sin firma real (la SPA no verifica la firma; eso lo hace la API). */
export function fakeJwt(claims: Record<string, unknown>) {
  const encode = (value: object) => btoa(JSON.stringify(value)).replace(/=+$/, '').replace(/\+/g, '-').replace(/\//g, '_');
  return `${encode({ alg: 'HS256', typ: 'JWT' })}.${encode(claims)}.firma`;
}

export const inOneHour = () => Math.floor(Date.now() / 1000) + 3600;

export const adminToken = () =>
  fakeJwt({ sub: '1', unique_name: 'admin', email: 'admin@distribucionpolar.local', name: 'Administrador del Sistema', role: 'Admin', exp: inOneHour() });

export const employeeToken = () =>
  fakeJwt({ sub: '2', unique_name: 'empleado', email: 'empleado@distribucionpolar.local', name: 'Empleado de Ventas', role: 'Employee', exp: inOneHour() });
