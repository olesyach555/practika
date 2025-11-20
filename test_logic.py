
import unittest

class TestMinOrderCalculation(unittest.TestCase):

    def test_order_needed(self):
        stock = 10
        min_count = 50
        in_pack = 10
        cost = 5.0

        needed = min_count - stock
        packages = (needed + in_pack - 1) // in_pack
        order_cost = packages * in_pack * cost

        self.assertEqual(order_cost, 200.0)

    def test_no_order_needed(self):
        stock = 60
        min_count = 50
        in_pack = 10
        cost = 5.0

        self.assertGreaterEqual(stock, min_count)

    def test_exact_amount_needed(self):
        stock = 40
        min_count = 50
        in_pack = 10
        cost = 5.0

        needed = min_count - stock
        packages = (needed + in_pack - 1) // in_pack
        order_cost = packages * in_pack * cost

        self.assertEqual(order_cost, 50.0)

if __name__ == '__main__':
    unittest.main()
