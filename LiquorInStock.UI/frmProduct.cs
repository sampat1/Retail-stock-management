﻿using Retail.Stock.Application.Common;
using Retail.Stock.Domain.Aggregates.Category;
using Retail.Stock.Domain.Aggregates.Product;
using Retail.Stock.UI.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Retail.Stock.UI
{
    public partial class frmProduct : Form
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private int _pageSize = 25;
        private int _pageIndex = 1;
        private int _totalPages = 1;
        private int _totalRecords = 0;

        private BindingSource _bindingSource = new BindingSource();
        public frmProduct(ICategoryRepository categoryRepository,
            IProductRepository productRepository)
        {
            InitializeComponent();

            _categoryRepository = categoryRepository;
            _productRepository = productRepository;
        }

        private void frmProduct_Load(object sender, EventArgs e)
        {
            _LoadCategory();
            LoadData();
        }

        private void _LoadCategory()
        {
            // Create a new item with the "Please select" text
            var pleaseSelectItem = new { Id = 0, Name = "Please select" };

            // Get the list of categories from the repository
            List<Category> categories = _Category();

            // Insert the "Please select" item at the beginning of the list

            // Bind the list of categories to the ComboBox control
            cmbCategory.DataSource = categories;
            cmbCategory.DisplayMember = "CategoryName";
            cmbCategory.ValueMember = "Id";


        }

        private List<Category> _Category()
        {
            return _categoryRepository.GetAll().ToList();
        }


        private void LoadData(int? categoryId = null, string product = "")
        {
            var category = _Category();
            // Get the data from your repository, filtered and sorted as needed
            var products = _productRepository.GetPage(_pageIndex, _pageSize, product, categoryId);
            var data = products.Result.Select(x => new ProductModel()
            {

                ProductId = x.Id,
                ProductName = x.ProductName,
                RetailPrice = x.RetailPrice,
                StockIn = x.StockIn,
                CategoryName = category.Where(s => s.Id.Equals(x.CategoryId)).FirstOrDefault()?.CategoryName,
            }).ToList();

            // Get the total number of records
            _totalRecords = products.TotalPage;

            // Calculate the total number of pages
            _totalPages = (int)Math.Ceiling((double)_totalRecords / _pageSize);

            // Set the data source of the binding source
            _bindingSource.DataSource = data;

            // Bind the binding source to the data grid view
            dataGridView1.DataSource = _bindingSource;
            // dataGridView1.AutoGenerateColumns = false;
            // Show the row numbers


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

        private void btnFirstPage_Click(object sender, EventArgs e)
        {
            if (_pageIndex != 1)
            {
                _pageIndex = 1;
                LoadData();
            }
        }

        private void btnPreviousPage_Click(object sender, EventArgs e)
        {
            if (_pageIndex > 1)
            {
                _pageIndex--;
                LoadData();
            }
        }

        private void btnNextPage_Click(object sender, EventArgs e)
        {
            if (_pageIndex < _totalPages)
            {
                _pageIndex++;
                LoadData();
            }
        }

        private void btnLastPage_Click(object sender, EventArgs e)
        {
            if (_pageIndex != _totalPages)
            {
                _pageIndex = _totalPages;
                LoadData();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                Category selectedCategory = (Category)cmbCategory.SelectedItem;
                // validate the form inputs
                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    throw new Exception("Please enter a category name.");
                }
                if (!string.IsNullOrEmpty(TxtId.Text))
                {
                    var product = _productRepository.GetById(Convert.ToInt32(TxtId.Text));
                    product.SetProduct(selectedCategory.Id, txtName.Text);
                    _productRepository.Update(product);
                }
                else
                {
                    // Create a new product with the selected category and name
                    Product newProduct = new Product(selectedCategory.Id, txtName.Text);

                    // Save the new product to the repository
                    _productRepository.Add(newProduct);


                }

                // Display a message box to indicate that the product was saved
                MessageBox.Show("Product saved successfully.");

                // clear the form inputs
                txtName.Clear();
                TxtId.Clear();
                LoadData();
            }
            catch (Exception ex)
            {
                // show an error message if something goes wrong
                MessageBox.Show($"An error occurred: {ex.Message}");

            }


        }

        private void button4_Click(object sender, EventArgs e)
        {
            LoadData();
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                // Get the selected row's data
                var product = (ProductModel)_bindingSource[e.RowIndex];

                // Fill the win form with the selected row's data
                txtName.Text = product.ProductName;
                TxtId.Text = product.ProductId.ToString();
                cmbCategory.SelectedItem = product.CategoryName;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Category selectedCategory = (Category)cmbCategory.SelectedItem;

            LoadData(selectedCategory.Id, txtName.Text);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            var selectedProduct = dataGridView1.CurrentRow?.DataBoundItem as ProductModel;

            if (selectedProduct != null)
            {
                // Ask the user for confirmation
                var result = MessageBox.Show($"Are you sure you want to delete {selectedProduct.ProductName}?",
                    "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        // Delete the product from the database
                        _productRepository.Remove(selectedProduct.ProductId);

                        // Refresh the data
                        _pageIndex = 1;
                        LoadData();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"An error occurred while deleting the product: {ex.Message}",
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a product to delete.", "Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}