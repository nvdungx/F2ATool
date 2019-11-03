/*
 * Sample.c
 * Description : the sample to evaluate your solution
 */

/*
**********************************************************************************************************************
* Includes
**********************************************************************************************************************
*/
#include <stdio.h>

/*
**********************************************************************************************************************
* Function prototypes
**********************************************************************************************************************
*/
void Sample_3(void);

/*
**********************************************************************************************************************
* Function Definitions
**********************************************************************************************************************
*/
uint8 temp[3] = {{1,2}, {4,5}, 4,8};
void tempFunc(void){
  int a = 10;
  printf("asdd; asad\" { 22}");
  while(a>1){
    a--;
    }
    }

void Sample1(void) //+10 Point
{
	void (*fn) \
  (void) = 0;
	int a = 0;
	int b = 1;

	fn = Sample_3;
	int sum = a + b;

	struct {
		int x;
		int y;
	} cord = { 0 };

	cord.x = 10;
	cord.y = 20;

	printf("%d\n", sum);
}

void Sample_2(int a) //+20 Point
{
	int b = 0;

	if (0 == a)
	{
		b++;
	}
	else if (a > 10)
	{
		--b;
	}
	else
	{
		b = a;
	}

	// switch(a)
	// {
		// case 0:
			// b = 5;
			// break;
		// case 10:
			// b = 25;
			// break;
		// case 20:
			// b = 50;
			// break;
		// case 30:
		// case 40:
		// default:
			// b++;
			// break;
	// }
}

void Sample_3(void) //+30 Point
{
	unsigned int array[8] = {0};

	for (int i; i < 8; i++)
	{
		array[i] = 0xFAU;
	}

	int i;
	while (i < 8)
	{
		array[i] += 1;
		i++; 
	}

	do
	{
		array[i]++;
		i++;
	} while (i < 8);

	while (i < 8)
	{
		if (4 == i) continue;
		array[i]--;
		i++;
	}
}

void Sample_4(void) //+40 Point
{
	unsigned int array[8] = {0};


	for (int i; i < 8; i++)
	{
		if (i < 2)
		{
			continue;
		}
		else
		{
			if (i > 4)
			{
				array[i] = 0xF0U;

			}
			else if (7 == i)
			{
				array[i] = 0xFFU;
			}
			else
			{
				array[i] = 0xFAU;
			}
		}
	}
}
