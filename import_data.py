
import pandas as pd
import sqlite3

def import_data():
    conn = sqlite3.connect('materials.db')
    cursor = conn.cursor()

    # Clear existing data to avoid duplicates
    cursor.execute('DELETE FROM MaterialSupplier')
    cursor.execute('DELETE FROM Material')
    cursor.execute('DELETE FROM Supplier')
    cursor.execute('DELETE FROM MaterialType')
    conn.commit()

    # Import MaterialType
    df_material_type = pd.read_excel('Material_type_import.xlsx')
    df_material_type = df_material_type[['Тип материала']]
    df_material_type.rename(columns={'Тип материала': 'Title'}, inplace=True)
    df_material_type.to_sql('MaterialType', conn, if_exists='append', index=False)
    print("MaterialType imported.")

    # Get MaterialType map for foreign key reference
    material_type_map = pd.read_sql('SELECT ID, Title FROM MaterialType', conn).set_index('Title')['ID'].to_dict()

    # Import Material
    df_material = pd.read_excel('Materials_import.xlsx')
    df_material['MaterialTypeID'] = df_material['Тип материала'].map(material_type_map)
    df_material.rename(columns={
        'Наименование материала': 'Title',
        'Количество на складе': 'CountInStock',
        'Единица измерения': 'Unit',
        'Количество в упаковке': 'CountInPack',
        'Минимальное количество': 'MinCount',
        'Цена единицы материала': 'Cost'
    }, inplace=True)
    df_material = df_material[['Title', 'MaterialTypeID', 'CountInStock', 'Unit', 'CountInPack', 'MinCount', 'Cost']]
    df_material.to_sql('Material', conn, if_exists='append', index=False)
    print("Material imported.")

    # Import Supplier
    df_supplier = pd.read_excel('Suppliers_import.xlsx')
    df_supplier.rename(columns={
        'Наименование поставщика': 'Title',
        'ИНН': 'INN',
        'Тип поставщика': 'SupplierType'
    }, inplace=True)
    df_supplier = df_supplier[['Title', 'INN', 'SupplierType']]
    df_supplier.to_sql('Supplier', conn, if_exists='append', index=False)
    print("Supplier imported.")

    # Get Material and Supplier maps for foreign key reference
    material_map = pd.read_sql('SELECT ID, Title FROM Material', conn).set_index('Title')['ID'].to_dict()
    supplier_map = pd.read_sql('SELECT ID, Title FROM Supplier', conn).set_index('Title')['ID'].to_dict()

    # Import MaterialSupplier
    df_material_supplier = pd.read_excel('Material_suppliers_import.xlsx')
    df_material_supplier['MaterialID'] = df_material_supplier['Наименование материала'].map(material_map)
    df_material_supplier['SupplierID'] = df_material_supplier['Поставщик'].map(supplier_map)
    df_material_supplier = df_material_supplier[['MaterialID', 'SupplierID']]
    df_material_supplier.to_sql('MaterialSupplier', conn, if_exists='append', index=False)
    print("MaterialSupplier imported.")

    conn.close()

if __name__ == '__main__':
    import_data()
    print("Data imported successfully.")
