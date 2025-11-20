
from eralchemy2 import render_er

def main():
    render_er("sqlite:///materials.db", "ERD.pdf")
    print("ER diagram generated successfully.")

if __name__ == "__main__":
    main()
