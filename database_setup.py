
import sqlite3

def create_database():
    conn = sqlite3.connect('materials.db')
    cursor = conn.cursor()

    # Create MaterialType table
    cursor.execute('''
    CREATE TABLE IF NOT EXISTS MaterialType (
        ID INTEGER PRIMARY KEY AUTOINCREMENT,
        Title TEXT NOT NULL
    )
    ''')

    # Create Material table
    cursor.execute('''
    CREATE TABLE IF NOT EXISTS Material (
        ID INTEGER PRIMARY KEY AUTOINCREMENT,
        Title TEXT NOT NULL,
        MaterialTypeID INTEGER,
        CountInStock INTEGER,
        Unit TEXT,
        CountInPack INTEGER,
        MinCount INTEGER,
        Cost REAL,
        FOREIGN KEY (MaterialTypeID) REFERENCES MaterialType(ID)
    )
    ''')

    # Create Supplier table
    cursor.execute('''
    CREATE TABLE IF NOT EXISTS Supplier (
        ID INTEGER PRIMARY KEY AUTOINCREMENT,
        Title TEXT NOT NULL,
        INN TEXT,
        SupplierType TEXT
    )
    ''')

    # Create MaterialSupplier table
    cursor.execute('''
    CREATE TABLE IF NOT EXISTS MaterialSupplier (
        MaterialID INTEGER,
        SupplierID INTEGER,
        PRIMARY KEY (MaterialID, SupplierID),
        FOREIGN KEY (MaterialID) REFERENCES Material(ID),
        FOREIGN KEY (SupplierID) REFERENCES Supplier(ID)
    )
    ''')

    conn.commit()
    conn.close()

if __name__ == '__main__':
    create_database()
    print("Database and tables created successfully.")
