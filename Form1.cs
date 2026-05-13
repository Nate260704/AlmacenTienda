using System;
using System.Data;
using System.Windows.Forms;
using AlmacenTienda.DataAccess;
using AlmacenTienda.Models;

namespace AlmacenTienda
{
    public partial class FrmProductos : Form
    {
        // ── instancia del DAL (Persona 1 lo creó) ──────────────────────────
        private ProductoDAL productoDAL = new ProductoDAL();

        // ID del producto seleccionado en la tabla (0 = ninguno)
        private int idProductoSeleccionado = 0;

        public FrmProductos()
        {
            InitializeComponent();
        }

        // ════════════════════════════════════════════════════════════════════
        //  CARGA INICIAL
        // ════════════════════════════════════════════════════════════════════
        private void FrmProductos_Load(object sender, EventArgs e)
        {
            CargarProductos();
        }

        // ════════════════════════════════════════════════════════════════════
        //  MÉTODO: cargar todos los productos en la tabla
        // ════════════════════════════════════════════════════════════════════
        private void CargarProductos()
        {
            try
            {
                DataTable dt = productoDAL.ListarProductos();
                dgvProductos.DataSource = dt;
                lblConteo.Text = $"Total: {dt.Rows.Count} productos";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar productos: " + ex.Message,
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ════════════════════════════════════════════════════════════════════
        //  BOTÓN BUSCAR
        // ════════════════════════════════════════════════════════════════════
        private void btnBuscar_Click(object sender, EventArgs e)
        {
            string texto = txtBuscar.Text.Trim();

            try
            {
                if (string.IsNullOrEmpty(texto))
                {
                    // Si no escribió nada, muestra todos
                    CargarProductos();
                }
                else
                {
                    DataTable dt = productoDAL.BuscarProductos(texto);
                    dgvProductos.DataSource = dt;
                    lblConteo.Text = $"Resultados: {dt.Rows.Count} producto(s)";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al buscar: " + ex.Message,
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ════════════════════════════════════════════════════════════════════
        //  BOTÓN GUARDAR (Insertar o Actualizar)
        // ════════════════════════════════════════════════════════════════════
        private void btnGuardar_Click(object sender, EventArgs e)
        {
            // ── Validaciones básicas ─────────────────────────────────────
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("El nombre del producto es obligatorio.",
                                "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNombre.Focus();
                return;
            }

            if (!decimal.TryParse(txtPrecioVenta.Text, out decimal precioVenta) || precioVenta <= 0)
            {
                MessageBox.Show("Ingresa un precio de venta válido (número mayor a 0).",
                                "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPrecioVenta.Focus();
                return;
            }

            if (!decimal.TryParse(txtPrecioCompra.Text, out decimal precioCompra) || precioCompra < 0)
            {
                MessageBox.Show("Ingresa un precio de compra válido.",
                                "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPrecioCompra.Focus();
                return;
            }

            if (!int.TryParse(txtStock.Text, out int stock) || stock < 0)
            {
                MessageBox.Show("El stock debe ser un número entero mayor o igual a 0.",
                                "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtStock.Focus();
                return;
            }

            // ── Armar el objeto Producto ─────────────────────────────────
            Producto p = new Producto
            {
                NombreProducto = txtNombre.Text.Trim(),
                CodigoBarras   = txtCodigo.Text.Trim(),
                Descripcion    = txtDescripcion.Text.Trim(),
                PrecioCompra   = precioCompra,
                PrecioVenta    = precioVenta,
                StockActual    = stock,
                StockMinimo    = (int)nudStockMin.Value,
                StockMaximo    = (int)nudStockMax.Value,
                IdCategoria    = (int)cmbCategoria.SelectedValue,
                IdMarca        = (int)cmbMarca.SelectedValue,
                IdProveedor    = (int)cmbProveedor.SelectedValue,
                Activo         = true
            };

            try
            {
                bool resultado;

                if (idProductoSeleccionado == 0)
                {
                    // ── INSERTAR ─────────────────────────────────────────
                    resultado = productoDAL.InsertarProducto(p);
                    if (resultado)
                        MessageBox.Show("Producto guardado correctamente.",
                                        "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // ── ACTUALIZAR ────────────────────────────────────────
                    p.IdProducto = idProductoSeleccionado;
                    resultado = productoDAL.ActualizarProducto(p);
                    if (resultado)
                        MessageBox.Show("Producto actualizado correctamente.",
                                        "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                if (resultado)
                {
                    LimpiarFormulario();
                    CargarProductos();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar: " + ex.Message,
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ════════════════════════════════════════════════════════════════════
        //  BOTÓN ELIMINAR
        // ════════════════════════════════════════════════════════════════════
        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (idProductoSeleccionado == 0)
            {
                MessageBox.Show("Selecciona un producto de la tabla primero.",
                                "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult confirm = MessageBox.Show(
                $"¿Estás seguro de eliminar el producto: {txtNombre.Text}?",
                "Confirmar eliminación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                try
                {
                    bool resultado = productoDAL.EliminarProducto(idProductoSeleccionado);

                    if (resultado)
                    {
                        MessageBox.Show("Producto eliminado correctamente.",
                                        "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LimpiarFormulario();
                        CargarProductos();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al eliminar: " + ex.Message,
                                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ════════════════════════════════════════════════════════════════════
        //  BOTÓN RESTAR STOCK (lógica de venta rápida)
        // ════════════════════════════════════════════════════════════════════
        private void btnRestarStock_Click(object sender, EventArgs e)
        {
            if (idProductoSeleccionado == 0)
            {
                MessageBox.Show("Selecciona un producto de la tabla primero.",
                                "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Pedir la cantidad a descontar
            string input = Microsoft.VisualBasic.Interaction.InputBox(
                "¿Cuántas unidades deseas vender/descontar?",
                "Restar Stock",
                "1");

            if (string.IsNullOrEmpty(input)) return;

            if (!int.TryParse(input, out int cantidad) || cantidad <= 0)
            {
                MessageBox.Show("Ingresa una cantidad válida (número entero mayor a 0).",
                                "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Verificar que haya suficiente stock
            int stockActual = (int)nudStock.Value; // tomamos el valor mostrado en pantalla
            if (cantidad > stockActual)
            {
                MessageBox.Show($"No hay suficiente stock. Stock actual: {stockActual}",
                                "Stock insuficiente", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Obtener el producto, restar stock y actualizar
                Producto p = productoDAL.ObtenerProductoPorId(idProductoSeleccionado);
                p.StockActual -= cantidad;

                bool resultado = productoDAL.ActualizarProducto(p);

                if (resultado)
                {
                    MessageBox.Show($"Stock actualizado. Nuevo stock: {p.StockActual}",
                                    "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Advertir si quedó bajo mínimo
                    if (p.StockActual <= p.StockMinimo)
                        MessageBox.Show($"⚠ El stock del producto está por debajo del mínimo ({p.StockMinimo}).",
                                        "Stock bajo", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    LimpiarFormulario();
                    CargarProductos();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al restar stock: " + ex.Message,
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ════════════════════════════════════════════════════════════════════
        //  BOTÓN VER BAJO STOCK (reporte simple)
        // ════════════════════════════════════════════════════════════════════
        private void btnBajoStock_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable dt = productoDAL.ProductosBajoStock();
                dgvProductos.DataSource = dt;
                lblConteo.Text = $"Productos con bajo stock: {dt.Rows.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message,
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ════════════════════════════════════════════════════════════════════
        //  BOTÓN LIMPIAR
        // ════════════════════════════════════════════════════════════════════
        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();
        }

        // ════════════════════════════════════════════════════════════════════
        //  CLIC EN LA TABLA → llena el formulario
        // ════════════════════════════════════════════════════════════════════
        private void dgvProductos_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow fila = dgvProductos.Rows[e.RowIndex];

            try
            {
                idProductoSeleccionado = Convert.ToInt32(fila.Cells["ID"].Value);

                Producto p = productoDAL.ObtenerProductoPorId(idProductoSeleccionado);
                if (p == null) return;

                txtCodigo.Text       = p.CodigoBarras;
                txtNombre.Text       = p.NombreProducto;
                txtDescripcion.Text  = p.Descripcion;
                txtPrecioCompra.Text = p.PrecioCompra.ToString("F2");
                txtPrecioVenta.Text  = p.PrecioVenta.ToString("F2");
                txtStock.Text        = p.StockActual.ToString();
                nudStockMin.Value    = p.StockMinimo;
                nudStockMax.Value    = p.StockMaximo;
                nudStock.Value       = p.StockActual;

                lblModoEdicion.Text      = "✏ Modo: EDITAR";
                lblModoEdicion.ForeColor = System.Drawing.Color.Orange;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar datos: " + ex.Message);
            }
        }

        // ════════════════════════════════════════════════════════════════════
        //  BÚSQUEDA EN TIEMPO REAL al presionar Enter en el textbox
        // ════════════════════════════════════════════════════════════════════
        private void txtBuscar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                btnBuscar_Click(sender, e);
        }

        // ════════════════════════════════════════════════════════════════════
        //  HELPER: limpiar formulario
        // ════════════════════════════════════════════════════════════════════
        private void LimpiarFormulario()
        {
            idProductoSeleccionado = 0;
            txtCodigo.Text         = "";
            txtNombre.Text         = "";
            txtDescripcion.Text    = "";
            txtPrecioCompra.Text   = "";
            txtPrecioVenta.Text    = "";
            txtStock.Text          = "0";
            nudStockMin.Value      = 0;
            nudStockMax.Value      = 0;
            nudStock.Value         = 0;
            txtBuscar.Text         = "";

            lblModoEdicion.Text      = "➕ Modo: NUEVO";
            lblModoEdicion.ForeColor = System.Drawing.Color.Green;

            txtNombre.Focus();
        }
    }
}
