from setuptools import setup, find_packages

setup(
    name='records',
    version='0.0.1',
    description='A framework to help python developers to work with the Record Framework.',
    author='example',
    author_email='ex@example.com',
    url='https://github.com/equinor/records',
    packages=find_packages(),
    classifiers=[],
    install_requires=[
        "rdflib"
    ],
    python_requires='>=3.12',
)