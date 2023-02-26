﻿using Retail.Stock.Application.Common;
using Retail.Stock.Domain.Aggregates.Category;
using Retail.Stock.Domain.Aggregates.Product;
using Retail.Stock.UI.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Retail.Stock.UI
{
    public partial class frmProductPrice : Form
    {
        private readonly IProductPriceRepository _productPriceRepository;
        private readonly IProductRepository _productRepository;
        private int _pageSize = 25;
        private int _pageIndex = 1;
        private int _totalPages = 1;
        private int _totalRecords = 0;
        private BindingSource _bindingSource = new BindingSource();
        private DateTime _startDate = DateTime.Today.AddDays(-15);
        private DateTime _endDate = DateTime.Today.AddDays(1);
        private int? _productId = null;
        public frmProductPrice(IProductPriceRepository productPriceRepository, IProductRepository productRepository)
        {
            _productPriceRepository = productPriceRepository;
            InitializeComponent();
            _productRepository = productRepository;
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedValue = cmbType.SelectedItem?.ToString();
            if (selectedValue == "Carton")
            {
                txtPerQuantity.Visible = true;
                label3.Visible = true;
                txtPerQuantity.Text = string.Empty;
                txtCartonQuantity.Visible = true;
                label4.Visible = true;
                txtCartonQuantity.Text = string.Empty;

                txtCartonPrice.Visible = true;
                label2.Visible = true;
                txtCartonPrice.Text = string.Empty;
            }
            else
            {
                txtPerQuantity.Visible = false;
                label3.Visible = false;
                txtPerQuantity.Text = string.Empty;

                txtCartonQuantity.Visible = false;
                label4.Visible = false;
                txtCartonQuantity.Text = string.Empty;

                txtCartonPrice.Visible = false;
                label2.Visible = false;
                txtCartonPrice.Text = string.Empty;
            }
        }

        private void frmProductPrice_Load(object sender, EventArgs e)
        {
            dateTimePicker1.Value = _startDate; dateTimePicker2.Value = _endDate;

            cmbType.SelectedIndex = 0;
            _LoadProduct();
            _LoadProductForSearch();

            comboBox1.SelectedIndex = -1;

            LoadData();
        }

        private void _LoadProduct()
        {

            // Get the list of categories from the repository
            IEnumerable<Product> products = _productRepository.GetAll();

            // Insert the "Please select" item at the beginning of the list

            // Bind the list of categories to the ComboBox control
            cmbProduct.DataSource = products;
            cmbProduct.DisplayMember = "ProductName";
            cmbProduct.ValueMember = "Id";

        }

        private void _LoadProductForSearch()
        {

            // Get the list of categories from the repository
            IEnumerable<Product> products = _productRepository.GetAll();

            // Insert the "Please select" item at the beginning of the list

            // Bind the list of categories to the ComboBox control
            comboBox1.DataSource = products;
            comboBox1.DisplayMember = "ProductName";
            comboBox1.ValueMember = "Id";
        }

        private void txtQuantity_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtPerQuantity_TextChanged(object sender, EventArgs e)
        {
            var selectedValue = cmbType.SelectedItem?.ToString();
            if (selectedValue == "Carton" && !string.IsNullOrEmpty(txtPerQuantity.Text)
                && !string.IsNullOrEmpty(txtCartonQuantity.Text) && !string.IsNullOrEmpty(txtCartonPrice.Text))
            {
                int totalQuantity = (Convert.ToInt32(txtPerQuantity.Text) * Convert.ToInt32(txtCartonQuantity.Text));
                txtQuantity.Text = totalQuantity.ToString();
                txtPrice.Text = (Convert.ToDecimal(txtCartonPrice.Text) / Convert.ToDecimal(totalQuantity)).ToString("F2");
            }
        }

        private void txtCartonQuantity_Click(object sender, EventArgs e)
        {
            var selectedValue = cmbType.SelectedItem?.ToString();
            if (selectedValue == "Carton" && !string.IsNullOrEmpty(txtCartonQuantity.Text)
                && !string.IsNullOrEmpty(txtPerQuantity.Text) && !string.IsNullOrEmpty(txtCartonPrice.Text))
            {
                int totalQuantity = (Convert.ToInt32(txtCartonQuantity.Text) * Convert.ToInt32(txtPerQuantity.Text));
                txtQuantity.Text = totalQuantity.ToString();
                txtPrice.Text = (Convert.ToDecimal(txtCartonPrice.Text) / Convert.ToDecimal(totalQuantity)).ToString("F2");
            }
        }

        private void txtCartonPrice_TextChanged(object sender, EventArgs e)
        {
            var selectedValue = cmbType.SelectedItem?.ToString();
            if (selectedValue == "Carton" && !string.IsNullOrEmpty(txtCartonPrice.Text)
                && !string.IsNullOrEmpty(txtPerQuantity.Text) && !string.IsNullOrEmpty(txtCartonQuantity.Text))
            {
                int totalQuantity = (Convert.ToInt32(txtCartonQuantity.Text) * Convert.ToInt32(txtPerQuantity.Text));
                txtQuantity.Text = totalQuantity.ToString();
                txtPrice.Text = (Convert.ToDecimal(txtCartonPrice.Text) / Convert.ToDecimal(totalQuantity)).ToString("F2");
            }
        }
        private void LoadData()
        {
            _startDate = dateTimePicker1.Value;
            _endDate = dateTimePicker2.Value;
            _productId = string.IsNullOrEmpty(comboBox1.SelectedValue?.ToString()) ? null : Convert.ToInt32(comboBox1.SelectedValue);
            var products = _productPriceRepository
                .GetPage(_pageIndex, _pageSize, _productId, _startDate, _endDate);
            List<Product> productsList = _productRepository.GetAll().ToList();
            var data = products.Result.Select(x => new ProductPriceModel()
            {

                ProductPriceId = x.Id,
                ProductName = productsList.Where(s => s.Id.Equals(x.ProductId)).FirstOrDefault()?.ProductName,
                Quantity = x.Quantity,
                PurchasedPrice = x.Price,
                TotaPurchasedPrice = x.Price * x.Quantity,
                SellingPrice = x.SellingPrice,
                TotalSellingPrice = x.SellingPrice * x.Quantity,
            }).ToList();
            _totalRecords = products.TotalPage;

            // Calculate the total number of pages
            _totalPages = (int)Math.Ceiling((double)_totalRecords / _pageSize);

            // Set the data source of the binding source
            _bindingSource.DataSource = data;

            // Bind the binding source to the data grid view
            dataGridView1.DataSource = _bindingSource;
            // dataGridView1.AutoGenerateColumns = false;
            // Show the row numbers

            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.CellFormatting += (sender, e) =>
            {
                if (e.ColumnIndex == 0 && e.RowIndex >= 0)
                {
                    e.Value = (e.RowIndex + 1) + ((_pageIndex - 1) * _pageSize);
                    e.FormattingApplied = false;
                }
            };

            dataGridView1.CurrentCellChanged += (sender, e) =>
            {
                var currentPage = _pageIndex;
                var totalPages = _totalPages;
                var totalRecords = _totalRecords;
                var pageSize = _pageSize;

                var displayInfo = string.Format("Page {0} of {1} ({2} records per page, {3} total records)",
                    currentPage, totalPages, pageSize, totalRecords);
                toolStripStatusLabel1.Text = displayInfo;
            };
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                Product selected = (Product)cmbProduct.SelectedItem;
                if (selected is null)
                {
                    throw new Exception("Please enter a Product.");
                }
                // validate the form inputs
                else if (string.IsNullOrWhiteSpace(txtQuantity.Text))
                {
                    throw new Exception("Please enter a  quantity.");
                }
                else if (string.IsNullOrWhiteSpace(txtSellingPrice.Text))
                {
                    throw new Exception("Please enter a  selling price.");
                }
                else if (string.IsNullOrWhiteSpace(txtPrice.Text))
                {
                    throw new Exception("Please enter a  price.");

                }
                var product = _productRepository.GetById(selected.Id);
                if (string.IsNullOrEmpty(txId.Text))
                {
                    ProductPrice _productPrice = new(selected.Id, Convert.ToInt32(txtQuantity.Text),
                        Convert.ToDecimal(txtPrice.Text), Convert.ToDecimal(txtSellingPrice.Text));
                    product.SetPurchasedPrice(Convert.ToDecimal(txtPrice.Text));
                    product.SetRetailPrice(Convert.ToDecimal(txtSellingPrice.Text));
                    product.SetStockIn(Convert.ToInt32(txtQuantity.Text));

                    _productPrice.AddProduct(product);

                    _productPriceRepository.Add(_productPrice);
                    _productRepository.Update(product);
                }
                else
                {
                    ProductPrice productPrice = _productPriceRepository.GetById(int.Parse(txId.Text));

                    productPrice.SetDetail(
                   int.Parse(txId.Text),
                   int.Parse(txtQuantity.Text),
                   decimal.Parse(txtPrice.Text),
                   decimal.Parse(txtSellingPrice.Text));

                    product.SetPurchasedPrice(Convert.ToDecimal(txtPrice.Text));
                    product.SetRetailPrice(Convert.ToDecimal(txtSellingPrice.Text));
                    int remainingQuentity = (product.StockIn - productPrice.Quantity) + int.Parse(txtQuantity.Text);


                    _productPriceRepository.Update(productPrice);
                    _productRepository.Update(product);
                }


                MessageBox.Show("Product price saved successfully.");
                txId.Clear();
                txtQuantity.Clear();
                txtPrice.Clear();
                txtSellingPrice.Clear();
                txtCartonPrice.Clear();
                txtCartonQuantity.Clear();
                txtPerQuantity.Clear();

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        private void btnFirstPage_Click(object sender, EventArgs e)
        {
            if (_pageIndex != 1)
            {
                _pageIndex = 1;
                LoadData();
            }
        }

        private void BtnPreviousPage_Click(object sender, EventArgs e)
        {
            if (_pageIndex > 1)
            {
                _pageIndex--;
                LoadData();
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (_pageIndex < _totalPages)
            {
                _pageIndex++;
                LoadData();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (_pageIndex != _totalPages)
            {
                _pageIndex = _totalPages;
                LoadData();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            LoadData();
        }
    }
}