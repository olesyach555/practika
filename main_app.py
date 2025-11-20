
import sys
import sqlite3
from PyQt5.QtWidgets import (QApplication, QMainWindow, QTableView, QVBoxLayout,
                             QWidget, QPushButton, QDialog, QFormLayout, QLineEdit,
                             QComboBox, QMessageBox, QLabel)
from PyQt5.QtSql import QSqlDatabase, QSqlTableModel, QSqlQuery
from PyQt5.QtGui import QIcon, QPixmap

class EditDialog(QDialog):
    def __init__(self, parent=None, material_id=None):
        super().__init__(parent)
        self.material_id = material_id
        self.initUI()
        if self.material_id:
            self.load_data()

    def initUI(self):
        self.setWindowTitle('Add/Edit Material')
        layout = QFormLayout()

        self.title_edit = QLineEdit()
        self.type_combo = QComboBox()
        self.stock_edit = QLineEdit()
        self.unit_edit = QLineEdit()
        self.pack_edit = QLineEdit()
        self.min_count_edit = QLineEdit()
        self.cost_edit = QLineEdit()

        self.load_material_types()

        layout.addRow('Title:', self.title_edit)
        layout.addRow('Type:', self.type_combo)
        layout.addRow('Stock:', self.stock_edit)
        layout.addRow('Unit:', self.unit_edit)
        layout.addRow('In Pack:', self.pack_edit)
        layout.addRow('Min Count:', self.min_count_edit)
        layout.addRow('Cost:', self.cost_edit)

        self.save_button = QPushButton('Save')
        self.save_button.clicked.connect(self.save_data)
        layout.addRow(self.save_button)

        self.setLayout(layout)

    def load_material_types(self):
        query = QSqlQuery("SELECT ID, Title FROM MaterialType")
        while query.next():
            self.type_combo.addItem(query.value(1), query.value(0))

    def load_data(self):
        query = QSqlQuery()
        query.prepare("SELECT * FROM Material WHERE ID = ?")
        query.addBindValue(self.material_id)
        query.exec_()
        if query.next():
            self.title_edit.setText(query.value(1))

            type_id = query.value(2)
            index = self.type_combo.findData(type_id)
            if index != -1:
                self.type_combo.setCurrentIndex(index)

            self.stock_edit.setText(str(query.value(3)))
            self.unit_edit.setText(query.value(4))
            self.pack_edit.setText(str(query.value(5)))
            self.min_count_edit.setText(str(query.value(6)))
            self.cost_edit.setText(str(query.value(7)))

    def save_data(self):
        title = self.title_edit.text()
        type_id = self.type_combo.currentData()
        stock = self.stock_edit.text()
        unit = self.unit_edit.text()
        in_pack = self.pack_edit.text()
        min_count = self.min_count_edit.text()
        cost = self.cost_edit.text()

        if not all([title, stock, unit, in_pack, min_count, cost]):
            QMessageBox.warning(self, 'Error', 'All fields must be filled.')
            return

        try:
            stock = int(stock)
            in_pack = int(in_pack)
            min_count = int(min_count)
            cost = float(cost)
        except ValueError:
            QMessageBox.warning(self, 'Error', 'Invalid data type for numeric fields.')
            return

        if min_count < 0 or cost < 0:
            QMessageBox.warning(self, 'Error', 'Min count and cost cannot be negative.')
            return

        query = QSqlQuery()
        if self.material_id:
            query.prepare("""
                UPDATE Material SET Title = ?, MaterialTypeID = ?, CountInStock = ?,
                Unit = ?, CountInPack = ?, MinCount = ?, Cost = ? WHERE ID = ?
            """)
            query.addBindValue(title)
            query.addBindValue(type_id)
            query.addBindValue(stock)
            query.addBindValue(unit)
            query.addBindValue(in_pack)
            query.addBindValue(min_count)
            query.addBindValue(cost)
            query.addBindValue(self.material_id)
        else:
            query.prepare("""
                INSERT INTO Material (Title, MaterialTypeID, CountInStock, Unit,
                CountInPack, MinCount, Cost) VALUES (?, ?, ?, ?, ?, ?, ?)
            """)
            query.addBindValue(title)
            query.addBindValue(type_id)
            query.addBindValue(stock)
            query.addBindValue(unit)
            query.addBindValue(in_pack)
            query.addBindValue(min_count)
            query.addBindValue(cost)

        if query.exec_():
            self.accept()
        else:
            QMessageBox.critical(self, 'Error', 'Failed to save data: ' + query.lastError().text())


class MainWindow(QMainWindow):
    def __init__(self):
        super().__init__()
        self.initUI()
        self.load_materials()

    def initUI(self):
        self.setWindowTitle('Material Management')
        self.setWindowIcon(QIcon('icon.ico'))

        central_widget = QWidget()
        self.setCentralWidget(central_widget)
        layout = QVBoxLayout(central_widget)

        logo_label = QLabel()
        pixmap = QPixmap('logo.png')
        logo_label.setPixmap(pixmap)
        layout.addWidget(logo_label)

        self.table_view = QTableView()
        layout.addWidget(self.table_view)

        self.add_button = QPushButton('Add Material')
        self.add_button.clicked.connect(self.add_material)
        layout.addWidget(self.add_button)

        self.edit_button = QPushButton('Edit Material')
        self.edit_button.clicked.connect(self.edit_material)
        layout.addWidget(self.edit_button)

        self.suppliers_button = QPushButton('View Suppliers')
        self.suppliers_button.clicked.connect(self.view_suppliers)
        layout.addWidget(self.suppliers_button)

        self.calculate_button = QPushButton('Calculate Min Order')
        self.calculate_button.clicked.connect(self.calculate_min_order)
        layout.addWidget(self.calculate_button)

        self.setGeometry(100, 100, 800, 600)

    def calculate_min_order(self):
        selected_row = self.table_view.currentIndex().row()
        if selected_row >= 0:
            record = self.model.record(selected_row)
            stock = record.value('CountInStock')
            min_count = record.value('MinCount')
            in_pack = record.value('CountInPack')
            cost = record.value('Cost')

            if stock < min_count:
                needed = min_count - stock
                packages = (needed + in_pack - 1) // in_pack
                order_cost = packages * in_pack * cost
                QMessageBox.information(self, 'Min Order Cost', f'The minimum order cost is: {order_cost:.2f}')
            else:
                QMessageBox.information(self, 'Min Order Cost', 'No order needed, stock is sufficient.')

    def load_materials(self):
        self.model = QSqlTableModel(self, self.db)
        self.model.setTable('Material')
        self.model.setEditStrategy(QSqlTableModel.OnManualSubmit)
        self.model.select()
        self.model.setHeaderData(1, 1, 'Title')
        self.model.setHeaderData(3, 1, 'Stock')
        self.model.setHeaderData(7, 1, 'Cost')
        self.table_view.setModel(self.model)

    def add_material(self):
        dialog = EditDialog(self)
        if dialog.exec_() == QDialog.Accepted:
            self.load_materials()

    def edit_material(self):
        selected_row = self.table_view.currentIndex().row()
        if selected_row >= 0:
            material_id = self.model.record(selected_row).value('ID')
            dialog = EditDialog(self, material_id)
            if dialog.exec_() == QDialog.Accepted:
                self.load_materials()

    def view_suppliers(self):
        selected_row = self.table_view.currentIndex().row()
        if selected_row >= 0:
            material_id = self.model.record(selected_row).value('ID')
            self.suppliers_dialog = SuppliersDialog(material_id)
            self.suppliers_dialog.show()


class SuppliersDialog(QDialog):
    def __init__(self, material_id):
        super().__init__()
        self.material_id = material_id
        self.initUI()
        self.load_suppliers()

    def initUI(self):
        self.setWindowTitle('Suppliers')
        layout = QVBoxLayout()
        self.table_view = QTableView()
        layout.addWidget(self.table_view)
        self.setLayout(layout)

    def load_suppliers(self):
        model = QSqlTableModel(self)
        model.setTable('Supplier')

        query = QSqlQuery()
        query.prepare("""
            SELECT s.* FROM Supplier s
            JOIN MaterialSupplier ms ON s.ID = ms.SupplierID
            WHERE ms.MaterialID = ?
        """)
        query.addBindValue(self.material_id)
        query.exec_()

        model.setQuery(query)
        self.table_view.setModel(model)


def main():
    app = QApplication(sys.argv)

    app.setStyleSheet("""
        QWidget {
            font-family: 'Comic Sans MS';
            background-color: #FFFFFF;
        }
        QMainWindow {
            background-color: #ABCFCE;
        }
        QDialog {
            background-color: #ABCFCE;
        }
        QPushButton {
            background-color: #546F94;
            color: white;
            border-radius: 5px;
            padding: 5px;
        }
        QLineEdit, QComboBox, QTableView {
            background-color: #FFFFFF;
        }
        QLabel {
            background-color: #ABCFCE;
        }
    """)

    db = QSqlDatabase.addDatabase('QSQLITE')
    db.setDatabaseName('materials.db')
    if not db.open():
        QMessageBox.critical(None, 'Error', 'Could not open database')
        return -1

    main_window = MainWindow()
    main_window.db = db
    main_window.show()

    sys.exit(app.exec_())

if __name__ == '__main__':
    main()
