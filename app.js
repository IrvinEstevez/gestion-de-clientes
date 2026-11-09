// JavaScript envía las solicitudes HTTP; C# procesa el CRUD en el servidor.
const formulario = document.getElementById("formularioCliente");
const tabla = document.getElementById("cuerpoTabla");
const mensaje = document.getElementById("mensaje");
const idCliente = document.getElementById("idCliente");
const cancelar = document.getElementById("cancelar");
const titulo = document.getElementById("tituloFormulario");
const guardar = document.getElementById("guardar");

function avisar(texto, error = false) {
    mensaje.textContent = texto;
    mensaje.className = error ? "error" : "ok";
}

async function solicitar(ruta, opciones = {}) {
    const respuesta = await fetch(ruta, opciones);
    let datos = {};
    try { datos = await respuesta.json(); } catch { /* Respuesta sin JSON */ }
    if (!respuesta.ok) {
        throw new Error(datos.mensaje || `Error HTTP ${respuesta.status}. Revisa el servidor C# y XAMPP.`);
    }
    return datos;
}

function obtenerDatosFormulario() {
    const campos = ["nombres", "apellidos", "identificacion", "correo", "telefono",
        "direccion", "ciudad", "provincia", "fechaNacimiento", "estado"];
    const cliente = {};
    for (const campo of campos) {
        cliente[campo] = document.getElementById(campo).value.trim();
    }
    return cliente;
}

function limpiarFormulario() {
    formulario.reset();
    idCliente.value = "";
    titulo.textContent = "Registrar cliente";
    guardar.textContent = "Guardar cliente";
    cancelar.hidden = true;
}

function crearBoton(texto, clase, alPulsar) {
    const boton = document.createElement("button");
    boton.type = "button";
    boton.textContent = texto;
    boton.className = `boton pequeno ${clase}`;
    boton.addEventListener("click", alPulsar);
    return boton;
}

async function cargarClientes() {
    try {
        const clientes = await solicitar("/api/clientes");
        tabla.replaceChildren();
        if (clientes.length === 0) {
            const fila = tabla.insertRow();
            const celda = fila.insertCell();
            celda.colSpan = 12;
            celda.textContent = "Todavía no hay clientes registrados.";
            return;
        }
        for (const cliente of clientes) {
            const fila = tabla.insertRow();
            const valores = [cliente.id, cliente.nombres, cliente.apellidos,
                cliente.identificacion, cliente.correo, cliente.telefono,
                cliente.direccion, cliente.ciudad, cliente.provincia,
                cliente.fechaNacimiento, cliente.estado];
            for (const valor of valores) {
                fila.insertCell().textContent = valor ?? "";
            }
            const acciones = fila.insertCell();
            acciones.append(
                crearBoton("Editar", "", () => editarCliente(cliente.id)),
                crearBoton("Eliminar", "peligro", () => eliminarCliente(cliente.id, cliente.nombres))
            );
        }
    } catch (error) {
        tabla.replaceChildren();
        const fila = tabla.insertRow();
        const celda = fila.insertCell();
        celda.colSpan = 12;
        celda.textContent = "No se pudieron cargar los clientes.";
        avisar(error.message, true);
    }
}

async function editarCliente(id) {
    try {
        const cliente = await solicitar(`/api/clientes/${id}`);
        idCliente.value = cliente.id;
        for (const campo of ["nombres", "apellidos", "identificacion", "correo", "telefono",
            "direccion", "ciudad", "provincia", "fechaNacimiento", "estado"]) {
            document.getElementById(campo).value = cliente[campo];
        }
        titulo.textContent = `Editar cliente #${id}`;
        guardar.textContent = "Guardar cambios";
        cancelar.hidden = false;
        avisar("Puedes modificar los datos y presionar Guardar cambios.");
        window.scrollTo({ top: 0, behavior: "smooth" });
    } catch (error) {
        avisar(error.message, true);
    }
}

async function eliminarCliente(id, nombre) {
    if (!confirm(`¿Deseas eliminar a ${nombre}? Esta acción no se puede deshacer.`)) return;
    try {
        const respuesta = await solicitar(`/api/clientes/${id}`, { method: "DELETE" });
        if (Number(idCliente.value) === id) limpiarFormulario();
        avisar(respuesta.mensaje);
        await cargarClientes();
    } catch (error) {
        avisar(error.message, true);
    }
}

formulario.addEventListener("submit", async (evento) => {
    evento.preventDefault();
    const id = idCliente.value;
    const editar = id !== "";
    const ruta = editar ? `/api/clientes/${id}` : "/api/clientes";
    try {
        guardar.disabled = true;
        const respuesta = await solicitar(ruta, {
            method: editar ? "PUT" : "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(obtenerDatosFormulario())
        });
        limpiarFormulario();
        avisar(respuesta.mensaje);
        await cargarClientes();
    } catch (error) {
        avisar(error.message, true);
    } finally {
        guardar.disabled = false;
    }
});

cancelar.addEventListener("click", () => {
    limpiarFormulario();
    avisar("Edición cancelada.");
});

cargarClientes();
